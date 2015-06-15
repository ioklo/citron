using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    public class ValueType : DefaultType
    {
        public IType Type
        {
            get { return GlobalDomain.TypeType; }
        }
    }
}
