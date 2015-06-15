using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.Runtime
{
    public class ExternFunction : IFunction
    {
        public string Name { get; private set; }
        public IType RetType { get; private set; }
        public IReadOnlyList<IType> ArgTypes { get; private set; }
        public Func<IValue[], IValue> Instance;

        public ExternFunction(string name, IType retType, IEnumerable<IType> argTypes, Func<IValue[], IValue> instance)
        {
            Name = name;
            RetType = retType;
            ArgTypes = argTypes.ToList();
            Instance = instance;
        }

        public IType Type { get { return new FunctionType(RetType, ArgTypes); } }
    }
}
