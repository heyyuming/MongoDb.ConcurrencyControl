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
            var readTask = accountRepository.Read(account.AccountNumber);

            // this read will wait for the write to finish first. 
            var readCommittedTask = accountRepository.ReadCommitted(account.AccountNumber);

            await Task.WhenAll(writeTask, readTask, readCommittedTask);
            Console.WriteLine($"Task debit successfully.");
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
