using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency
{
    public class MongoDbContext
    {
        private const string ConnectionString = "mongodb://localhost:27017";

        private static Lazy<IMongoCollection<Account>> _collection =>
            new Lazy<IMongoCollection<Account>>(() => MongoClient.GetDatabase("PessimisticConcurrency").GetCollection<Account>(nameof(Account)));

        private static Lazy<MongoClient> _mongoClient =>
            new Lazy<MongoClient>(() => new MongoClient(ConnectionString));

        public static IMongoCollection<Account> Accounts
        {
            get { return _collection.Value; }
        }

        public static MongoClient MongoClient
        {
            get { return _mongoClient.Value; }
        }


        public static async Task UsingTransaction(
            Func<IClientSessionHandle, Task> action,
            ReadConcern readConcern = null,
            WriteConcern writeConcern = null)
        {

            readConcern ??= ReadConcern.Local;
            writeConcern ??= WriteConcern.W1;

            using var session = await MongoClient.StartSessionAsync();
            var transactionOptions = new TransactionOptions(readConcern: readConcern, writeConcern: writeConcern);
            session.StartTransaction(transactionOptions);
            try
            {
                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("Transaction started");
                await action(session);

                session.CommitTransaction();

                Console.WriteLine("Transaction Committed");
                Console.WriteLine("");
                Console.WriteLine("");
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
