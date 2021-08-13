using MongoDb.ConcurrencyControl.Data.Models;
using MongoDb.ConcurrencyControl.Data.Repositories;
using MongoDb.ConcurrencyControl.Exceptions;
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
        private static readonly PersonRepository personRepository = new PersonRepository();
        private static int ConcurrentThreads = 10;

        static async Task Main(string[] args)
        {
            //await ChangeNameProblem();

            //await IncrementingAgeProblem();

            //await OccWithVersion();

            //await OccWithVersionAndRetry();

            await TestOptimisticUsingThirdPartyLib();

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
                await Policy.Handle<ConcurrencyConflictException>().RetryForeverAsync().ExecuteAsync(async () =>
                {
                    var loaded = await personRepository.Get(person.Id);
                    var currentVersion = loaded.Version;

                    loaded.Age++;
                    loaded.Version++;

                    //Console.WriteLine($"Thread {i} age: {loaded.Age} version: {loaded.Version}");
                    await personRepository.UpdateWithConflict(loaded, currentVersion);
                });
            }).ToList();

            await Task.WhenAll(tasks);
            person = await personRepository.Get(person.Id);

            Console.WriteLine($"Actual Age      : {person.Age}");
            Console.WriteLine($"Expected Age    : {tasks.Count}");
        }

        private static async Task TestOptimisticUsingThirdPartyLib()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Alan",
                LastName = "Smith"
            };

            var optimisticRepo = new Data.Repositories.OptimisticUsingThirdPartyLib.PersonRepository();

            Console.WriteLine("Case 1: entity does not exist and upsert is false");
            try
            {
                await optimisticRepo.UpdateAsync(person, isUpsert: false);
            }
            catch (MongoConcurrencyDeletedException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Case 2: entity does not exist and upsert is true");
            await optimisticRepo.UpdateAsync(person, isUpsert: true);
            Console.WriteLine($"Result: inserted Person Id: {person.Id}");

            Console.WriteLine();
            Console.WriteLine("Case 3: update with no conflict");
            person = await personRepository.Get(person.Id);
            Console.WriteLine($"Version before update: {person.Version}");
            person.FirstName = "Bob";
            await optimisticRepo.UpdateAsync(person, isUpsert: false);
            Console.WriteLine($"Version after update: {person.Version}");

            Console.WriteLine();
            Console.WriteLine("Case 4: update with conflict");
            var personOtherThread = await personRepository.Get(person.Id);
            personOtherThread.FirstName = "Cameron";
            await optimisticRepo.UpdateAsync(personOtherThread, isUpsert: false);
            try
            {
                await optimisticRepo.UpdateAsync(person, isUpsert: false);
            }
            catch (MongoConcurrencyUpdatedException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
