using System;

namespace MongoDb.ConcurrencyControl.Data.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Version { get; set; }
    }
}
