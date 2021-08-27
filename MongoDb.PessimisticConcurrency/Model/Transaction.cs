using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.PessimisticConcurrency.Model
{
    class Transaction
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
    }
}
