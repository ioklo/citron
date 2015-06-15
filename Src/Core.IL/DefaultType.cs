using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class DefaultType : IType
    {
        public IReadOnlyList<IType> Fields { get; private set; }

        public DefaultType()
        {

        }

        public DefaultType(IEnumerable<IType> fields)
        {
            Fields = fields.ToList();
        }
    }
}
