using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    public class EmitResult
    {
        public List<Core.IL.ICommand> Commands { get; private set; }
        public List<int> JumpIndice { get; private set; }
        
        public EmitResult()
        {
            Commands = new List<Core.IL.ICommand>();
            JumpIndice = new List<int>();
        }

        public int NewPoint()
        {
            JumpIndice.Add(-1);
            return JumpIndice.Count - 1;
        }

        public void SetPoint(int t, int n)
        {
            JumpIndice[t] = n;
        }
    }
}
