using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency
{
    public class BulkAccountRepository
    {
        public Task Add(Account account)
        {
            return MongoDbContext.Accounts.InsertOneAsync(account);
        }

        public async Task<List<Account>> Read(List<string> accountNumbers)
        {
            await Task.Delay(1000);

            var accounts = (await MongoDbContext.Accounts.FindAsync(a => accountNumbers.Contains(a.AccountNumber))).ToList();

            Console.WriteLine("***************** Bulk Read Completed***********************");
            Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));
            Console.WriteLine("**************************** END **********************************");
            return accounts;
        }

        public async Task<List<Account>> ReadCommitted(List<string> accountNumbers)
        {
            await Task.Delay(1000);

            try
            {
                var update = Builders<Account>.Update.Set(a => a.ETag, Guid.NewGuid());

                var accounts = new List<Account>();
                foreach (var accountNumber in accountNumbers)
                {
                    var filter = Builders<Account>.Filter.Where(a => a.AccountNumber == accountNumber);

                    var account = await MongoDbContext.Accounts.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Account, Account>
                    {
                        MaxTime = TimeSpan.FromSeconds(10),
                        ReturnDocument = ReturnDocument.After
                    });

                    accounts.Add(account);
                }

                Console.WriteLine("***************** Bulk read committed data completed ***********************");
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));
                Console.WriteLine("********************************** END ****************************************");

                return accounts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadCommitted: {ex.Message}");
                throw;
            }
        }


        public async Task BulkWrite(List<Account> accounts, IClientSessionHandle session)
        {
            Console.WriteLine("Start bulk write");

            var updateModels = new List<UpdateOneModel<Account>>();

            foreach (var account in accounts)
            {
                var update = Builders<Account>.Update
                                                .Set(a => a.AccountName, account.AccountName)
                                                .Set(a => a.Balance, account.Balance);

                var filter = Builders<Account>.Filter.Where(a => a.AccountNumber == account.AccountNumber);

                var model = new UpdateOneModel<Account>(filter, update);

                updateModels.Add(model);
            }

            var result = await MongoDbContext.Accounts.BulkWriteAsync(session, updateModels);

            await Task.Delay(5000);
            Console.WriteLine("End bulk write");
        }
    }
}
