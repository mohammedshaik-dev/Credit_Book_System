using Credit_Book_System.Models;

namespace Credit_Book_System.Interface.IRepository
{
    public interface ICreditBookRepository
    {
        Task<IEnumerable<CreditEntry>> GetCreditEntriesAsync();
        Task<CreditEntry?> GetCreditEntryByIdAsync(int id);
        Task AddCreditEntryAsync(CreditEntry entry);
        Task UpdateCreditEntryAsync(CreditEntry entry);
        Task DeleteCreditEntryAsync(int id);
        Task<IEnumerable<Settlement>> GetSettlementsByCreditEntryIdAsync();
        Task AddSettlementAsync(Settlement settlement);
        Task<decimal> GetTotalExpensesAsync();
        Task<decimal> GetTotalPaidAmountAsync();
        Task<decimal> GetRemainingBalanceAsync();
    }
}
