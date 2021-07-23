using System;

namespace MongoDb.ConcurrencyControl.Data.Models
{
    public interface IPerson
    {
        int Age { get; set; }
        string FirstName { get; set; }
        string Id { get; set; }
        string LastName { get; set; }
    }

    public class Person : IPerson
    {
        public string Id { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
