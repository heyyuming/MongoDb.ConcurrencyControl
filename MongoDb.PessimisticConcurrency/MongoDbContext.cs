using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using System;

namespace MongoDb.PessimisticConcurrency
{
    public class MongoDbContext
    {
        private const string ConnectionString = "mongodb://localhost:27017";

        private static Lazy<IMongoCollection<Account>> _accounts => 
            new Lazy<IMongoCollection<Account>>(() => MongoClient.GetDatabase("PessimisticConcurrency").GetCollection<Account>(nameof(Account)));

        private static Lazy<IMongoCollection<Transaction>> _transactions =>
            new Lazy<IMongoCollection<Transaction>>(() => MongoClient.GetDatabase("PessimisticConcurrency").GetCollection<Transaction>(nameof(Transaction)));

        private static Lazy<MongoClient> _mongoClient => 
            new Lazy<MongoClient>(() => new MongoClient(ConnectionString));

        public static IMongoCollection<Account> Accounts
        {
            get { return _accounts.Value; }
        }

        internal static IMongoCollection<Transaction> Transactions
        {
            get { return _transactions.Value; }
        }

        public static MongoClient MongoClient
        {
            get { return _mongoClient.Value; }
        }
    }
}
