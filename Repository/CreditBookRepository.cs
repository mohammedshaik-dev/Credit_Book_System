using Credit_Book_System.Interface.IRepository;
using Credit_Book_System.Models;
using Npgsql;
using Dapper;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Credit_Book_System.Repository
{
    public class CreditBookRepository : ICreditBookRepository
    {
        private readonly string _connectionString;

        public CreditBookRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<IEnumerable<CreditEntry>> GetCreditEntriesAsync()
        {
            const string query = "SELECT  id, date, itemname, quantity, rate, totalamount, remainingbalance FROM CreditEntries ORDER BY Date DESC";
            using var connection = CreateConnection();
            return await connection.QueryAsync<CreditEntry>(query);
        }

        public async Task<CreditEntry?> GetCreditEntryByIdAsync(int id)
        {
            const string query = "SELECT * FROM CreditEntries WHERE Id = @Id";
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<CreditEntry>(query, new { Id = id });
        }

        public async Task AddCreditEntryAsync(CreditEntry entry)
        {
            const string query = @"INSERT INTO CreditEntries (Date, ItemName, Quantity, Rate, RemainingBalance) 
                              VALUES (@Date, @ItemName, @Quantity, @Rate, @RemainingBalance) RETURNING Id";
            using var connection = CreateConnection();
            entry.Id = await connection.ExecuteScalarAsync<int>(query, entry);
        }

        public async Task UpdateCreditEntryAsync(CreditEntry entry)
        {
            const string query = @"UPDATE CreditEntries SET 
                              Date = @Date, 
                              ItemName = @ItemName, 
                              Quantity = @Quantity, 
                              Rate = @Rate, 
                              RemainingBalance = @RemainingBalance 
                              WHERE Id = @Id";
            using var connection = CreateConnection();
            await connection.ExecuteAsync(query, entry);
        }

        public async Task DeleteCreditEntryAsync(int id)
        {
            const string query = "DELETE FROM CreditEntries WHERE Id = @Id";
            using var connection = CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<Settlement>> GetSettlementsByCreditEntryIdAsync()
        {
            const string query = "SELECT * FROM Settlements ORDER BY Date DESC";
            using var connection = CreateConnection();
            return await connection.QueryAsync<Settlement>(query);
        }

        public async Task<decimal> GetTotalExpensesAsync()
        {
            const string query = "SELECT COALESCE(SUM(totalamount), 0) FROM public.creditentries";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(query);
        }

        public async Task<decimal> GetTotalPaidAmountAsync()
        {
            const string query = "SELECT COALESCE(SUM(amountpaid), 0) FROM public.settlements";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(query);
        }

        public async Task<decimal> GetRemainingBalanceAsync()
        {
            const string query = @"SELECT COALESCE(SUM(c.totalamount) - SUM(s.amountpaid), 0) FROM public.creditentries c 
                                    LEFT JOIN public.settlements s ON c.id = s.creditentryid";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(query);
        }

        public async Task AddSettlementAsync(Settlement settlement)
        {
            // Open a database connection.
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Begin a transaction to ensure all updates occur atomically.
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // 1. Retrieve all credit entries (regardless of outstanding balance).
                const string selectAllCreditQuery = @"SELECT * FROM CreditEntries ORDER BY Date ASC";
                var allCreditEntries = (await connection.QueryAsync<CreditEntry>(
                    selectAllCreditQuery,
                    transaction: transaction)).ToList();

                // If no credit entries exist, inform the caller.
                if (!allCreditEntries.Any())
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("No credit entries exist. Please add a credit entry before recording a settlement.");
                }

                // 2. Filter out only those credit entries with an outstanding balance.
                var outstandingCreditEntries = allCreditEntries.Where(ce => ce.RemainingBalance > 0).ToList();
                if (!outstandingCreditEntries.Any())
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("All credit entries are fully paid. There is no outstanding balance to apply this settlement.");
                }
                // 2. Allocate the total payment amount across the outstanding entries.
                decimal remainingPayment = settlement.AmountPaid;

                foreach (var creditEntry in outstandingCreditEntries)
                {
                    // If the payment has been fully applied, break out of the loop.
                    if (remainingPayment <= 0)
                    {
                        break;
                    }

                    // Determine how much can be applied to this credit entry.
                    decimal allocation = Math.Min(creditEntry.RemainingBalance, remainingPayment);
                    decimal newRemainingBalance = creditEntry.RemainingBalance - allocation;

                    // 3. Update the credit entry's RemainingBalance.
                    const string updateCreditQuery = @"UPDATE CreditEntries SET RemainingBalance = @RemainingBalance WHERE Id = @Id";
                    await connection.ExecuteAsync(
                        updateCreditQuery,
                        new { RemainingBalance = newRemainingBalance, Id = creditEntry.Id },
                        transaction: transaction);

                    // 4. Insert a settlement record for this allocation.
                    const string insertSettlementQuery = @"INSERT INTO Settlements (Date, AmountPaid, RemainingBalance, CreditEntryId)
                                                           VALUES (@Date, @AmountPaid, @RemainingBalance, @CreditEntryId)";
                    await connection.ExecuteAsync(
                        insertSettlementQuery,
                        new Settlement  
                        {
                            Date = settlement.Date,
                            AmountPaid = allocation,
                            RemainingBalance = newRemainingBalance,
                            CreditEntryId = creditEntry.Id
                        },
                        transaction: transaction);

                    // Reduce the remaining payment amount.
                    remainingPayment -= allocation;
                }

                // Optional: Handle any surplus payment if it exceeds the total outstanding balance.
                if (remainingPayment > 0)
                {
                    // You might log a warning, alert the user, or handle it per your business logic.
                    // For example:
                    throw new InvalidOperationException("Partial payment exceeds the total outstanding balance.");
                }

                // 5. Commit the transaction once all allocations are complete.
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Roll back the transaction if any error occurs.
                await transaction.RollbackAsync();
                if (ex.Message.Contains("No credit entries exist. Please add a credit entry before recording a settlement."))
                {
                    throw new InvalidOperationException(ex.Message.ToString());
                }
                else if (ex.Message.Contains("All credit entries are fully paid. There is no outstanding balance to apply this settlement."))
                {
                    throw new InvalidOperationException(ex.Message.ToString());
                }
                else if (ex.Message.Contains("Partial payment exceeds the total outstanding balance."))
                {
                    throw new InvalidOperationException(ex.Message.ToString());
                }
                else
                {
                    throw new InvalidOperationException("An error occurred while recording the partial payment.", ex);
                }
            }
        }
    }
}
