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

            await Test();

            Console.Read();
        }

        private static async Task Test()
        {
            var accountRepo = new AccountRepository();
            var account = new Account
            {
                AccountNumber = Guid.NewGuid().ToString(),
                AccountName = "AlanSmith",
                Balance = 100.0m
            };

            await accountRepo.Add(account);

            var tasks = Enumerable.Range(0, 1).Select(async i =>
            {
                await accountRepo.Debit(account.AccountNumber, 10.0m);
            }).ToList();

            await Task.WhenAll(tasks);
        }


    }
}
