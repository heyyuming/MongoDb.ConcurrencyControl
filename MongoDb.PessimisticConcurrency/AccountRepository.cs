using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency
{
    public class AccountRepository
    {
        public Task Add(Account account)
        {
            return MongoDbContext.Accounts.InsertOneAsync(account);
        }

        public async Task<Account> Read(string accountNumber)
        {
            await Task.Delay(1000);

            var account = (await MongoDbContext.Accounts.FindAsync(a => a.AccountNumber == accountNumber)).FirstOrDefault();

            Console.WriteLine("***************** Simple Read Completed***********************");
            Console.WriteLine(JsonConvert.SerializeObject(account, Formatting.Indented));
            Console.WriteLine("**************************** END **********************************");
            return account;
        }

        public async Task<Account> ReadCommitted(string accountNumber)
        {
            await Task.Delay(1000);

            var updateDef = Builders<Account>.Update.Set(a => a.ETag, Guid.NewGuid());

            var account = await MongoDbContext.Accounts.FindOneAndUpdateAsync(a => a.AccountNumber == accountNumber, updateDef);

            Console.WriteLine("***************** Read with read concern completed Start ***********************");
            Console.WriteLine(JsonConvert.SerializeObject(account, Formatting.Indented));
            Console.WriteLine("********************************** END ****************************************");

            return account;
        }

        public async Task Debit(string accountNumber, decimal amount, IClientSessionHandle session)
        {
            var updateDef = Builders<Account>.Update.Set(a => a.AccountName, "abc").Inc(a => a.Balance, -amount);

            var loadedAccount = await MongoDbContext.Accounts.FindOneAndUpdateAsync(session, a => a.AccountNumber == accountNumber, updateDef);

            await Task.Delay(5000);
        }


        public async Task Debit(string accountNumber, decimal amount)
        {
            using var session = await MongoDbContext.MongoClient.StartSessionAsync().ConfigureAwait(false);
            var transactionOptions = new TransactionOptions(readConcern: ReadConcern.Local, writeConcern: WriteConcern.W1);
            session.StartTransaction(transactionOptions);

            try
            {
                var updateDef = Builders<Account>.Update.Set(a => a.ETag, Guid.NewGuid());

                var loadedAccount = await MongoDbContext.Accounts.FindOneAndUpdateAsync(session, a => a.AccountNumber == accountNumber, updateDef);

                loadedAccount.Balance -= amount;

                await MongoDbContext.Accounts.ReplaceOneAsync(session, a => a.AccountNumber == accountNumber, loadedAccount, new ReplaceOptions { IsUpsert = false });

                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
