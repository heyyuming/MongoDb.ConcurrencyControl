using MongoDb.ConcurrencyControl.Data;
using MongoDb.ConcurrencyControl.Data.Models;
using MongoDb.ConcurrencyControl.Data.Repositories;
using MongoDB.Concurrency.Optimistic;
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
            MongoDbContext.RegisterClassMap();

            await TestProxyWrapper();

            Console.Read();
        }

        private async static Task TestProxyWrapper()
        {
            IPerson person = new Person
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Alan",
                LastName = "Smith",
                Age = 20
            };

            Console.WriteLine("Test 1: insert new person");
            person = await PersonRepository.UpsertAsync(person);
            Console.WriteLine($"Inserted person: {PrintPerson(person)}");

            Console.WriteLine();
            Console.WriteLine("Test 2: update person");
            Console.WriteLine($"Before update: {PrintPerson(person)}");
            person.Age++;
            person = await PersonRepository.UpsertAsync(person);
            Console.WriteLine($"After update: {PrintPerson(person)}");

            Console.WriteLine();
            Console.WriteLine("Test 3: update conflict");
            var thread1 = await PersonRepository.Get(person.Id);
            var thread2 = await PersonRepository.Get(person.Id);

            thread1.FirstName = "Bob";
            thread1 = await PersonRepository.UpsertAsync(thread1);
            Console.WriteLine($"thread 1 update: {PrintPerson(thread1)}");

            try
            {
                thread2.LastName = "Carter";
                await PersonRepository.UpsertAsync(thread2);
            }
            catch (ConflictException ex)
            {
                Console.WriteLine($"thread 2 update: {ex.Message}");
            }
        }

        private static string PrintPerson(IPerson person)
        {
            if (person is VersionControlProxy<IPerson> vcp)
                return $"Version: {vcp.Version} - {JsonConvert.SerializeObject(vcp.Target)} " ;

            return JsonConvert.SerializeObject(person);
        }
    }
}
