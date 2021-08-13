using MongoDb.ConcurrencyControl.Data.Models;
using MongoDB.Concurrency;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl.Data.Repositories.OptimisticUsingThirdPartyLib
{
    //use library from https://github.com/Anapher/MongoDB.Concurrency
    /*
     1. Entity class must have a property of type int (typically called Version)
     2. When updating an entity, use the .Optimistic() extension method and supply a reference to the version property
    */
    public class PersonRepository
    {
        public async Task UpdateAsync(Person person, bool isUpsert)
        {
            await MongoDbContext.Persons
                .Optimistic(p => p.Version)
                .UpdateAsync(person, new ReplaceOptions { IsUpsert = isUpsert });
        }
    }
}
