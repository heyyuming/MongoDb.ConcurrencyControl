using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl.Data.Repositories
{
    public class BaseEntity<T>
    {
        [BsonId]
        public string Id { get; init; }

        [BsonElement("data")]
        public T Data { get; init; }

        [BsonElement("version")]
        public int Version { get; init; }

        [BsonElement("timestamp")]
        public string Timestamp { get; init; }


        [BsonConstructor]
        public BaseEntity(string id, T data, int version, string timestamp)
        {
            Id = id;
            Data = data;
            Version = version;
            Timestamp = timestamp;
        }
    }
}

