using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7CommPlusDriver.Legitimation
{
    public static class AccessLevel 
    {
        public const UInt32 FullAccess = 1;
        public const UInt32 ReadAccess = 2;
        public const UInt32 HMIAccess = 3;
        public const UInt32 NoAccess = 4;
    }
}
