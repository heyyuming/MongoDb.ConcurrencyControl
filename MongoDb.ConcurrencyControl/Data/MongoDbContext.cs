using MongoDb.ConcurrencyControl.Data.Models;
using MongoDB.Driver;
using System;

namespace MongoDb.ConcurrencyControl.Data
{
    public class MongoDbContext
    {
        private const string ConnectionString = "mongodb://localhost:27017";
        private static Lazy<IMongoCollection<Person>> _collection => Initialise();

        public static IMongoCollection<Person> Persons
        {
            get { return _collection.Value; }
        }

        private static Lazy<IMongoCollection<Person>> Initialise()
        {
            return new Lazy<IMongoCollection<Person>>(() =>
            {
                var client = new MongoClient(ConnectionString);
                var database = client.GetDatabase("ConcurrencyControlDemo");

                return database.GetCollection<Person>(nameof(Person));
            });
        }
    }
}
