using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDb.ConcurrencyControl.Data.Repositories
{
    public class VersionControlProxy<IPerson> : DispatchProxy
    { 
        public IPerson Target { get; private set; }
        internal int Version { get; private set; }

        public static IPerson Create(BaseEntity<IPerson> baseEntity)
        {
            var proxy = Create<IPerson, VersionControlProxy<IPerson>>();

            if (proxy is VersionControlProxy<IPerson> vcp)
            {
                vcp.Target = baseEntity.Data;
                vcp.Version = baseEntity.Version;
            }

            return proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            return targetMethod.Invoke(Target, args);
        }
    }
}
