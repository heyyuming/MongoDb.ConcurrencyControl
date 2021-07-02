using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await WithoutOCC();

            //await OccWithVersion();

            await OccWithVersionAndRetry();

            Console.Read();
        }

        private static async Task WithoutOCC()
        {
            var document = new Counter();
            await MongoDbContext.CounterCollection.InsertOneAsync(document);
            Console.WriteLine($"Before  : {document.Value}");

            var tasks = Enumerable.Range(0, 100).Select(async i =>
            {
                var loaded = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();

                loaded.Value++;

                long result;
                do
                {
                    result = (await MongoDbContext.CounterCollection.ReplaceOneAsync(c => c.Id == document.Id, loaded,
                        new ReplaceOptions { IsUpsert = false })).ModifiedCount;
                } while (result != 1);

                return result;
            }).ToList();

            var total = await Task.WhenAll(tasks);
            document = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();

            Console.WriteLine($"After   : {document.Value}");
            Console.WriteLine($"Modified: {total.Sum(r => r)}");
        }

        private static async Task OccWithVersion()
        {
            var document = new Counter();
            await MongoDbContext.CounterCollection.InsertOneAsync(document);
            Console.WriteLine($"Before  : {document.Value}");

            var tasks = Enumerable.Range(0, 100).Select(async i =>
            {
                var loaded = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();
                var version = loaded.Version;

                loaded.Value++;
                loaded.Version++;

                var result = await MongoDbContext.CounterCollection.ReplaceOneAsync(
                    c => c.Id == document.Id && c.Version == version,
                    loaded, new ReplaceOptions { IsUpsert = false });

                return result.ModifiedCount;
            }).ToList();

            var total = await Task.WhenAll(tasks);
            document = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();

            Console.WriteLine($"After   : {document.Value}");
            Console.WriteLine($"Modified: {total.Sum(r => r)}");
        }

        private static async Task OccWithVersionAndRetry()
        {
            var document = new Counter();
            await MongoDbContext.CounterCollection.InsertOneAsync(document);
            Console.WriteLine($"Before  : {document.Value}");

            var tasks = Enumerable.Range(0, 100).Select(async i =>
            {
                ReplaceOneResult result;

                do
                {
                    var loaded = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();
                    var version = loaded.Version;

                    loaded.Value++;
                    loaded.Version++;

                    result = await MongoDbContext.CounterCollection.ReplaceOneAsync(
                        c => c.Id == document.Id && c.Version == version, loaded,
                        new ReplaceOptions { IsUpsert = false });
                } while (result.ModifiedCount != 1);

                return result.ModifiedCount;
            }).ToList();

            var total = await Task.WhenAll(tasks);
            document = await MongoDbContext.CounterCollection.Find(doc => doc.Id == document.Id).SingleAsync();

            Console.WriteLine($"After   : {document.Value}");
            Console.WriteLine($"Modified: {total.Sum(r => r)}");
        }
    }
}
