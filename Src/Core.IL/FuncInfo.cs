using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class FuncInfo
    {
        public bool Extern { get; private set; }
        public string Name { get; private set; }
        public int ArgCount {get; private set;}
        public int RetValCount { get; private set; }
        public int LocalCount { get; private set; }
        public IReadOnlyList<int> JumpTable { get { return jumpTable; } }
        public IReadOnlyList<ICommand> Commands { get { return commands; } }

        List<int> jumpTable;
        List<ICommand> commands; 

        public FuncInfo(string name, int args, int returns)
        {
            Name = name;
            ArgCount = args;
            RetValCount = RetValCount;
            Debug.Assert(RetValCount <= 1);
            Extern = true;
            jumpTable = null;
            commands = null;
        }

        public FuncInfo(string name, int args, int locals, int returns, IEnumerable<ICommand> cmds, IEnumerable<int> jt)
        {
            Name = name;
            ArgCount = args;
            LocalCount = locals;
            RetValCount = returns;

            commands = new List<ICommand>(cmds);
            jumpTable = new List<int>(jt);
        }
    }
}
