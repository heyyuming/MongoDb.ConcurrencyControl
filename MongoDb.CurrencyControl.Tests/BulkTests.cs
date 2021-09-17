using MongoDb.PessimisticConcurrency;
using MongoDb.PessimisticConcurrency.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MongoDb.CurrencyControl.Tests
{
    public class BulkTests
    {
        private BulkAccountRepository _bulkAccountRepository = new BulkAccountRepository();

        [Fact]
        public async Task Bulk()
        {
            var account1 = new Account
            {
                AccountNumber = "1",
                AccountName = "Account 1",
                Balance = 100.0m
            };

            var account2 = new Account
            {
                AccountNumber = "2",
                AccountName = "Account 2",
                Balance = 200.0m
            };

            await _bulkAccountRepository.Add(account1);
            await _bulkAccountRepository.Add(account2);

            var writeTask = MongoDbContext.UsingTransaction(async (session) =>
            {
                account1.Balance -= 10;
                account2.Balance -= 10;

                await _bulkAccountRepository.BulkWrite(new List<Account> { account1, account2 }, session);
            });

            var readTask = _bulkAccountRepository.Read(new List<string> { account1.AccountNumber, account2.AccountNumber });

            var readCommittedTask = _bulkAccountRepository.ReadCommitted(new List<string> { account1.AccountNumber, account2.AccountNumber });

            await Task.WhenAll(writeTask, readTask, readCommittedTask);
        }
    }
}
