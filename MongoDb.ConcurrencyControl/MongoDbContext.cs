using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    public class MongoDbContext
    {
        private const string ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false";
        private static Lazy<IMongoCollection<Counter>> _collection => Initialise(); 

        public static IMongoCollection<Counter> CounterCollection
        {
            get { return _collection.Value; }
        }

        private static Lazy<IMongoCollection<Counter>> Initialise()
        {
            return new Lazy<IMongoCollection<Counter>>(() => 
            {
                var client = new MongoClient(ConnectionString);
                var database = client.GetDatabase("ConcurrencyControlDemo");

                return database.GetCollection<Counter>(nameof(Counter));
            });
        }

    }
}
