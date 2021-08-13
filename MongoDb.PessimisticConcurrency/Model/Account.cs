using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency.Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public Guid ETag { get; set; }
    }
}
