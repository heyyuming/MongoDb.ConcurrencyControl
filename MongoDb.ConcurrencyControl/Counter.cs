using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl
{
    public class Counter
    {
        public Guid Id { get; set; }
        public int Value { get; set; }

        public int Version { get; set; }
    }
}
