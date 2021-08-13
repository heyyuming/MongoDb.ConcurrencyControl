using MongoDb.PessimisticConcurrency;
using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    class Program
    {

        static async Task Main(string[] args)
        {

            await DebitAccount(false);

            Console.Read();
        }

        private static async Task DebitAccount(bool retry)
        {
            var accountRepo = new AccountRepository();
            var account = new Account
            {
                AccountNumber = Guid.NewGuid().ToString(),
                AccountName = "AlanSmith",
                Balance = 100.0m
            };

            await accountRepo.Add(account);

            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                await Policy.Handle<MongoDB.Driver.MongoCommandException>().RetryForeverAsync().ExecuteAsync(async () =>
                {
                    try
                    {
                        await accountRepo.Debit(account.AccountNumber, 10.0m);
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
