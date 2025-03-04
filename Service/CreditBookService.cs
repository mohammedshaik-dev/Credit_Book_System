﻿using Credit_Book_System.Interface.IRepository;
using Credit_Book_System.Interface.IService;
using Credit_Book_System.Models;

namespace Credit_Book_System.Service
{
    public class CreditBookService : ICreditBookService
    {
        private readonly ICreditBookRepository _repository;

        public CreditBookService(ICreditBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CreditEntry>> GetCreditEntriesAsync() => await _repository.GetCreditEntriesAsync();

        public async Task<CreditEntry?> GetCreditEntryByIdAsync(int id) => await _repository.GetCreditEntryByIdAsync(id);

        public async Task AddCreditEntryAsync(CreditEntry entry) => await _repository.AddCreditEntryAsync(entry);

        public async Task UpdateCreditEntryAsync(CreditEntry entry) => await _repository.UpdateCreditEntryAsync(entry);

        public async Task DeleteCreditEntryAsync(int id) => await _repository.DeleteCreditEntryAsync(id);

        public async Task<IEnumerable<Settlement>> GetSettlementsByCreditEntryIdAsync() => await _repository.GetSettlementsByCreditEntryIdAsync();

        public async Task AddSettlementAsync(Settlement settlement) => await _repository.AddSettlementAsync(settlement);

        public async Task<decimal> GetTotalExpensesAsync() => await _repository.GetTotalExpensesAsync();

        public async Task<decimal> GetTotalPaidAmountAsync() => await _repository.GetTotalPaidAmountAsync();

        public async Task<decimal> GetRemainingBalanceAsync() => await _repository.GetRemainingBalanceAsync();

    }

}
