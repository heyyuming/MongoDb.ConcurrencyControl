using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency
{
    public class AccountTransactionService
    {
        public async Task Transfer(Account from, Account to, decimal amount)
        {
            using var session = await MongoDbContext.MongoClient.StartSessionAsync().ConfigureAwait(false);
            var transactionOptions = new TransactionOptions(readConcern: ReadConcern.Majority, writeConcern: WriteConcern.WMajority);
            session.StartTransaction(transactionOptions);

            try
            {
                var fromAccount = await MongoDbContext.Accounts.Find(a => a.AccountNumber == from.AccountNumber).SingleAsync();
                var toAccount = await MongoDbContext.Accounts.Find(a => a.AccountNumber == to.AccountNumber).SingleAsync();

                if (fromAccount.Balance < amount)
                    throw new Exception($"Account - {fromAccount } does not have sufficient balance.");

                var updateDef = Builders<Account>.Update.Set(a => a.Balance, fromAccount.Balance - amount);
                await MongoDbContext.Accounts.UpdateOneAsync(
                    session,
                    a => a.AccountNumber == fromAccount.AccountNumber,
                    updateDef,
                    new UpdateOptions { IsUpsert = false });

                updateDef = Builders<Account>.Update.Set(a => a.Balance, to.Balance + amount);
                await MongoDbContext.Accounts.UpdateOneAsync(
                    session,
                    a => a.AccountNumber == toAccount.AccountNumber,
                    updateDef,
                    new UpdateOptions { IsUpsert = false });

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    From = fromAccount.AccountNumber,
                    To = toAccount.AccountNumber,
                    Amount = amount
                };
                await MongoDbContext.Transactions.InsertOneAsync(session, transaction);

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
