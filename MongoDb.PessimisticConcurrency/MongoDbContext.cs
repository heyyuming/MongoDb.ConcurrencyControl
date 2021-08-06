using MongoDb.PessimisticConcurrency.Model;
using MongoDB.Driver;
using System;

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
    }
}
