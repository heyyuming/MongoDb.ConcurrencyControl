using MongoDb.ConcurrencyControl.Data.Models;
using MongoDb.ConcurrencyControl.Exceptions;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl.Data.Repositories
{
    public class PersonRepository
    {
        public async Task Add(Person person)
        {
            await MongoDbContext.Persons.InsertOneAsync(person);
        }

        public async Task<Person> Get(Guid personId)
        {
            return await MongoDbContext.Persons.Find(p => p.Id == personId).SingleAsync();
        }

        public async Task<bool> Update(Person person)
        {
            var result = await MongoDbContext.Persons.ReplaceOneAsync(c => c.Id == person.Id, person, new ReplaceOptions { IsUpsert = false });

            return result.ModifiedCount == 1;
        }

        public async Task<bool> Update(Person person, int version)
        {
            var result = await MongoDbContext.Persons.ReplaceOneAsync(c => c.Id == person.Id && c.Version == version, person, new ReplaceOptions { IsUpsert = false });

            return result.ModifiedCount == 1;
        }

        public async Task UpdateWithConflict(Person person, int version)
        {
            var result = await MongoDbContext.Persons.ReplaceOneAsync(c => c.Id == person.Id && c.Version == version, person, new ReplaceOptions { IsUpsert = false });

            if (result.ModifiedCount != 1)
            {
                throw new ConcurrencyConflictException();
            }
        }
    }
}
