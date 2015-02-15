using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;
using Gum.Core.IL;

namespace Gum.App.Compiler
{
    public class CompilerContext
    {
        Stack<Tuple<int, int>> loopScopeInfo = new Stack<Tuple<int, int>>();

        // 레지스터 관리
        Stack<int> listSizes = new Stack<int>();
        List<string> locals = new List<string>();
        EmitResult emitResult = new EmitResult();

        int tempLocalCount;
        int maxLocalCount;

        // Global Table
        Dictionary<string, object> globalVariables = new Dictionary<string, object>();

        public Core.IL.Program Program { get; private set; }
        public TypeManager TypeManager { get; private set; }
        public EmitResult EmitResult { get { return emitResult; } }

        public CompilerContext()
        {
            Program = new Core.IL.Program();
            TypeManager = new TypeManager();

            foreach(var kv in TypeManager.Types)
            {
                Program.Types[kv.Key] = new Core.IL.TypeInfo(kv.Key, kv.Value.Fields.Count);
            }
        }

        public void PushLocalScope()
        {
            listSizes.Push(locals.Count);
        }

        public void PopLocalScope()
        {
            int count = listSizes.Pop();
            locals.RemoveRange(count, locals.Count - count);
        }


        public int AddLocal(string varName)
        {
            locals.Add(varName);
            maxLocalCount = Math.Max(maxLocalCount, locals.Count);
            return locals.Count - 1;
        }

        public int AddTempLocal()
        {
            int idx = AddLocal("@t" + tempLocalCount);
            tempLocalCount++;
            return idx;
        }

        public int GetLocal(string p)
        {
            return locals.LastIndexOf(p);
        }

        public void ClearLocal()
        {
            locals.Clear();
            listSizes.Clear();
            maxLocalCount = 0;
            tempLocalCount = 0;
        }

        public int MaxLocalCount
        {
            get { return maxLocalCount; }
        }

        
        // 루프 처리 Optional
        public int ContinuePoint
        {
            get
            {
                if (loopScopeInfo.Count < 1) return -1;
                return loopScopeInfo.Peek().Item1;
            }
        }
        public int BreakPoint
        {
            get
            {
                if (loopScopeInfo.Count < 1) return -1;
                return loopScopeInfo.Peek().Item2;
            }
        }
        
        public void AddGlobal(string name, object v)
        {
            globalVariables.Add(name, v);
        }

        public bool GetGlobal(string name, out object val)
        {
            return globalVariables.TryGetValue(name, out val);
        }

        public void PushLoopScope(int continuePoint, int breakPoint)
        {
            loopScopeInfo.Push(Tuple.Create(continuePoint, breakPoint));
        }

        public void PopLoopScope()
        {
            loopScopeInfo.Pop();
        }

        public void ClearLoopScope()
        {
            loopScopeInfo.Clear();
        }

        public void ClearJumpPoints()
        {
            EmitResult.JumpIndice.Clear();
        }
    }
}
