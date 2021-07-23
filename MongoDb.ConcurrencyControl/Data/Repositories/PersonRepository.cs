using MongoDb.ConcurrencyControl.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl.Data.Repositories
{
    public static class PersonRepository
    {
        public static async Task<IPerson> UpsertAsync(IPerson person)
        {
            BaseEntity<BsonDocument> documentToUpdate;
            int currentVersion, newVersion;
            Expression<Func<BaseEntity<BsonDocument>, bool>> filter;

            var options = new ReplaceOptions { IsUpsert = true };
            
            if (person is VersionControlProxy<IPerson> vcp)
            {
                currentVersion = vcp.Version;
                newVersion = currentVersion + 1;
                documentToUpdate = new BaseEntity<BsonDocument>(person.Id.ToString(), vcp.Target.ToBsonDocument(typeof(Person)), newVersion, GetTimestamp(DateTime.Now));
                filter = x => x.Id == person.Id && x.Version == currentVersion;
            }
            else
            {
                newVersion = 1;
                documentToUpdate = new BaseEntity<BsonDocument>(person.Id.ToString(), person.ToBsonDocument(typeof(Person)), newVersion, GetTimestamp(DateTime.Now));
                filter = x => x.Id == person.Id;
            }

            try
            {
                await MongoDbContext.Persons.ReplaceOneAsync
                    (
                        filter: filter,
                        replacement: documentToUpdate,
                        options: options
                    );
            }
            catch (MongoWriteException mwe) when (mwe.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // if upsert is enabled and the versions do not match, it will attempt to insert the new document
                // but as the ids of the documents are equal, this will throw duplicate key exception
                if (mwe.WriteError.Category == ServerErrorCategory.DuplicateKey && options.IsUpsert == true)
                {
                    throw new ConflictException();
                }

                throw;
            }

            var baseEntity = ToBaseEntity(documentToUpdate);
            return VersionControlProxy<IPerson>.Create(baseEntity);
        }

        public static async Task<IPerson> Get(string personId)
        {
            var doc = await MongoDbContext.Persons.Find(p => p.Id == personId.ToString()).SingleAsync();
            var baseEntity = ToBaseEntity(doc);
            return VersionControlProxy<IPerson>.Create(baseEntity);
        }

        private static BaseEntity<IPerson> ToBaseEntity(BaseEntity<BsonDocument> documentToUpdate)
        {
            return new BaseEntity<IPerson>(documentToUpdate.Id, BsonSerializer.Deserialize<Person>(documentToUpdate.Data), documentToUpdate.Version, documentToUpdate.Timestamp);
        }

        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
