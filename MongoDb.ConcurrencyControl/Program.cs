using MongoDb.ConcurrencyControl.Data.Models;
using MongoDb.ConcurrencyControl.Data.Repositories;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    class Program
    {
        private static readonly PersonRepository personRepository = new PersonRepository();
        private static int ConcurrentThreads = 10;

        static async Task Main(string[] args)
        {
            await ChangeNameProblem();

            await IncrementingAgeProblem();

            await OccWithVersion();

            await OccWithVersionAndRetry();

            Console.Read();
        }

        private static async Task IncrementingAgeProblem()
        {
            var person = new Person();
            await personRepository.Add(person);
            Console.WriteLine($"Age Before  : {person.Age}");

            var tasks = Enumerable.Range(0, ConcurrentThreads).Select(async i =>
            {
                var loaded = await personRepository.Get(person.Id);

                loaded.Age++;

                //Console.WriteLine($"Thread {i} age: {loaded.Age}");

                await personRepository.Update(loaded);
            }).ToList();

            await Task.WhenAll(tasks);

            person = await personRepository.Get(person.Id);

            Console.WriteLine($"Expcted Age : {tasks.Count}");
            Console.WriteLine($"Actual Age  : {person.Age}");
        }

        private static async Task ChangeNameProblem()
        {
            var person = new Person
            {
                FirstName = "John",
                LastName = "Smith"
            };

            await personRepository.Add(person);

            var thread1 = await personRepository.Get(person.Id);
            var thread2 = await personRepository.Get(person.Id);

            thread1.FirstName = "Jane";

            await personRepository.Update(person);

            person = await personRepository.Get(person.Id);

            thread2.LastName = "Doe";
            await personRepository.Update(person);

            person = await personRepository.Get(person.Id);

            Console.WriteLine("Final Result:");
            Console.WriteLine(JsonConvert.SerializeObject(person, Formatting.Indented));
        }

        private static async Task OccWithVersion()
        {
            var person = new Person();
            await personRepository.Add(person);
            Console.WriteLine($"Age Before      : {person.Age}");

            var tasks = Enumerable.Range(0, ConcurrentThreads).Select(async i =>
            {
                var loaded = await personRepository.Get(person.Id);
                var currentVersion = loaded.Version;

                loaded.Age++;
                loaded.Version++;

                return await personRepository.Update(loaded, currentVersion);

            }).ToList();

            await Task.WhenAll(tasks);
            person = await personRepository.Get(person.Id);

            Console.WriteLine($"Actual Age      : {person.Age}");
            Console.WriteLine($"Expected Age    : {tasks.Where(t => t.Result).Count()}");
        }


        private static async Task OccWithVersionAndRetry()
        {
            var person = new Person();
            await personRepository.Add(person);
            Console.WriteLine($"Age Before      : {person.Age}");

            var tasks = Enumerable.Range(0, ConcurrentThreads).Select(async i =>
            {
                bool result;

                do
                {
                    var loaded = await personRepository.Get(person.Id);
                    var currentVersion = loaded.Version;

                    loaded.Age++;
                    loaded.Version++;

                    //Console.WriteLine($"Thread {i} age: {loaded.Age} version: {loaded.Version}");

                    result = await personRepository.Update(loaded, currentVersion);

                } while (!result);

            }).ToList();

            await Task.WhenAll(tasks);
            person = await personRepository.Get(person.Id);

            Console.WriteLine($"Actual Age      : {person.Age}");
            Console.WriteLine($"Expected Age    : {tasks.Count}");
        }
    }
}
