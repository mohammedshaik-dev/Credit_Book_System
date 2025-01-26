﻿using Credit_Book_System.Models;

namespace Credit_Book_System.Interface.IService
{
    public interface ICreditBookService
    {
        Task<IEnumerable<CreditEntry>> GetCreditEntriesAsync();
        Task<CreditEntry?> GetCreditEntryByIdAsync(int id);
        Task AddCreditEntryAsync(CreditEntry entry);
        Task UpdateCreditEntryAsync(CreditEntry entry);
        Task DeleteCreditEntryAsync(int id);
        Task<IEnumerable<Settlement>> GetSettlementsByCreditEntryIdAsync(int creditEntryId);
        Task AddSettlementAsync(Settlement settlement);
    }

}
