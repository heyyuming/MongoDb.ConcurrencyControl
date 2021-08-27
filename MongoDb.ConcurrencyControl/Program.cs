using MongoDb.PessimisticConcurrency;
using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    class Program
    {
        static AccountRepository accountRepository = new AccountRepository();

        static async Task Main(string[] args)
        {
            var account = new Account
            {
                AccountNumber = Guid.NewGuid().ToString(),
                AccountName = "AlanSmith",
                Balance = 100.0m
            };

            await accountRepository.Add(account);



            //await DebitAccount(false, account);

            await DebitAccount(account);
            Console.Read();
        }


        private static async Task DebitAccount(Account account)
        {

            var writeTask = MongoDbContext.UsingTransaction(async (session) =>
            {
                await accountRepository.Debit(account.AccountNumber, 10.0m, session);
            });

            // this will read data without waiting for the data to be committed. 
            var availableTask = accountRepository.Read(account.AccountNumber, ReadConcern.Available);
            var linearizableTask = accountRepository.Read(account.AccountNumber, ReadConcern.Linearizable);
            var majorityTask = accountRepository.Read(account.AccountNumber, ReadConcern.Majority);
            var snapshotTask = accountRepository.Read(account.AccountNumber, ReadConcern.Snapshot);
            var localTask = accountRepository.Read(account.AccountNumber, ReadConcern.Local);

            await Task.WhenAll(writeTask, availableTask, linearizableTask, majorityTask, snapshotTask, localTask);

        }

        private static async Task DebitAccount(bool retry, Account account)
        {
            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                await Policy.Handle<MongoDB.Driver.MongoCommandException>().RetryForeverAsync().ExecuteAsync(async () =>
                {
                    try
                    {
                        await accountRepository.Debit(account.AccountNumber, 10.0m);
                        Console.WriteLine($"Task {i} debit successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Task {i} failed with: {ex.Message}");
                        if (retry)
                        {
                            throw;
                        }
                    }
                });
            }).ToList();

            await Task.WhenAll(tasks);
        }
    }
}
