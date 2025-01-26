using Credit_Book_System.Interface.IRepository;
using Credit_Book_System.Models;
using Npgsql;
using Dapper;
using System.Data;

namespace Credit_Book_System.Repository
{
    public class CreditBookRepository : ICreditBookRepository
    {
        private readonly string _connectionString;

        public CreditBookRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<IEnumerable<CreditEntry>> GetCreditEntriesAsync()
        {
            const string query = "SELECT * FROM CreditEntries ORDER BY Date DESC";
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

        public async Task<IEnumerable<Settlement>> GetSettlementsByCreditEntryIdAsync(int creditEntryId)
        {
            const string query = "SELECT * FROM Settlements WHERE CreditEntryId = @CreditEntryId ORDER BY Date DESC";
            using var connection = CreateConnection();
            return await connection.QueryAsync<Settlement>(query, new { CreditEntryId = creditEntryId });
        }

        public async Task AddSettlementAsync(Settlement settlement)
        {
            const string query = @"INSERT INTO Settlements (Date, AmountPaid, RemainingBalance, CreditEntryId) 
                              VALUES (@Date, @AmountPaid, @RemainingBalance, @CreditEntryId) RETURNING Id";
            using var connection = CreateConnection();
            settlement.Id = await connection.ExecuteScalarAsync<int>(query, settlement);
        }
    }
}
