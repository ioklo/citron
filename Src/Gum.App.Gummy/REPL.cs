using Gum.App.VM;
using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Gummy
{
    // 이 클래스가 여기 있어야 하는지는 모르겠지만 ㅎㅎㅎ
    public class REPL
    {
        public event Action<string> OnOutput;
        
        Gum.Core.IL.Domain env = new Core.IL.Domain();

        public REPL()
        {
            env.OnTypeAdded += OnTypeAdded;
            env.OnValueAdded += OnValueAdded;
        }

        void Write(string text, params object[] args)
        {
            if( OnOutput != null )
                OnOutput(string.Format(text, args));
        }

        void WriteLine(string text, params object[] args)
        {
            if (OnOutput != null)
                OnOutput(string.Format(text + System.Environment.NewLine, args));
        }


        internal void Init()
        {
            WriteLine("Gummy, GUM interactive");
            WriteLine("끝내려면 '#exit'를 입력해 주세요");
        }

        public async Task<string> Process(string input)
        {
            Interpreter i = new Interpreter();

            // 1. il이 나오고
            // 2. 변경된 환경이 나오고, 그걸 실행합니다
            // 3. Env가 변경된걸 감지할 수 있을까

            foreach(var cmd in Gum.App.Compiler.Compiler.CompileREPLStmt(env, input))
            {
                cmd.Visit(i);
            }

            return "";
        }

        private void OnValueAdded(string name, IValue value)
        {
            WriteLine(" - Value {0} added", name);
        }

        private void OnTypeAdded(string name, IType typeInfo)
        {
            WriteLine(" - Type {0} added", name);
        }
    }
}
