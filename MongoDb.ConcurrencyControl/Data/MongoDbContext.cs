using MongoDb.ConcurrencyControl.Data.Models;
using MongoDb.ConcurrencyControl.Data.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;

namespace MongoDb.ConcurrencyControl.Data
{
    public class MongoDbContext
    {
        private const string ConnectionString = "mongodb://localhost:27017";
        private static Lazy<IMongoCollection<BaseEntity<BsonDocument>>> _collection => Initialise();

        public static IMongoCollection<BaseEntity<BsonDocument>> Persons
        {
            get { return _collection.Value; }
        }

        private static Lazy<IMongoCollection<BaseEntity<BsonDocument>>> Initialise()
        {
            return new Lazy<IMongoCollection<BaseEntity<BsonDocument>>>(() =>
            {
                var client = new MongoClient(ConnectionString);
                var database = client.GetDatabase("ConcurrencyControlDemo");

                return database.GetCollection<BaseEntity<BsonDocument>>("ProxyWrapperDemo");
            });
        }

        internal static void RegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<Person>(cm =>
            {
                cm.MapMember(i => i.Id).SetElementName("id");
                cm.MapMember(i => i.FirstName).SetElementName("firstName");
                cm.MapMember(i => i.LastName).SetElementName("lastName");
                cm.MapMember(i => i.Age).SetElementName("age");

                cm.MapCreator(i => new Person { Id = i.Id, FirstName = i.FirstName, LastName = i.LastName, Age = i.Age });
            });
        }
    }
}
