using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MongoDb.PessimisticConcurrency.Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Balance { get; set; }
        public Guid ETag { get; set; }
    }
}
