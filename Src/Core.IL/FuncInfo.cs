using Gum.Core.IL.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    // 실제 함수 정보
    public class Function : IFunction
    {
        public string Name { get; private set; }
        public IType RetType { get; private set; }
        public IReadOnlyList<IType> ArgTypes { get; private set; }
        public IReadOnlyList<IType> LocalTypes { get; private set; }

        public Block StartBlock { get { return Blocks.First(); } }
        public IReadOnlyList<Block> Blocks { get; private set; }
        
        public Function(
            string name,
            IType retType, 
            IEnumerable<IType> argTypes,
            IEnumerable<IType> localTypes,             
            IEnumerable<Block> blocks)
        {
            Name = name;

            RetType = retType;
            ArgTypes = argTypes.ToList();
            LocalTypes = localTypes.ToList();

            Blocks = blocks.ToList();
        }
    }
}
