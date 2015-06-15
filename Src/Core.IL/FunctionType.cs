using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class FunctionType : IType
    {
        public IType RetType { get; private set; }
        public IReadOnlyList<IType> ArgTypes { get; private set; }
        public IReadOnlyList<IType> Fields
        {
            get
            {
                return fields;
            }
        }

        private IReadOnlyList<IType> fields = new List<IType>();
        
        public FunctionType(IType retType, IEnumerable<IType> argTypes)
        {
            RetType = retType;
            ArgTypes = argTypes.ToList();
        }
    }
}
