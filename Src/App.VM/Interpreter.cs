using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;

namespace Gum.App.VM
{
    public class Interpreter : ICommandVisitor
    {
        State state;
        Program prog;
        Dictionary<string, Func<object[], object>> externFuncMap = new Dictionary<string, Func<object[], object>>();

        public Interpreter(Program p)
        {
            prog = p;
            state = new State();
        }

        public void Visit(Push p)
        {
            state.Push(p.Value);
        }

        public void Visit(Dup p)
        {
            state.Dup();
        }

        public void Visit(Pop p)
        {
            state.Pop();
        }        

        // 
        public void Visit(New p)
        {
            var obj = new Object(p.TypeInfo.FieldCount);
            state.Push(obj);
        }

        public void Visit(LoadField p)
        {
            Object obj = (Object)state.Pop();
            state.Push(obj.Fields[p.FieldIndex]);
        }

        public void Visit(StoreField p)
        {
            Object obj = (Object)state.Pop();
            object val = state.Pop();

            obj.Fields[p.FieldIndex] = val;
        }

        public void Visit(Operator u)
        {
            switch (u.Kind)
            {
                case OperatorKind.Equal:
                    {
                        object i2 = state.Pop();
                        object i1 = state.Pop();

                        state.Push(i1.Equals(i2));

                        return;
                    }

                case OperatorKind.NotEqual:
                    {
                        object i2 = state.Pop();
                        object i1 = state.Pop();

                        state.Push(!i1.Equals(i2));

                        return;
                    }

                case OperatorKind.Less:
                    {
                        int i2 = (int)state.Pop();
                        int i1 = (int)state.Pop();

                        state.Push(i1 < i2);

                        return;
                    }

                case OperatorKind.Greater:
                    {
                        int i2 = (int)state.Pop();
                        int i1 = (int)state.Pop();

                        state.Push(i1 > i2);
                        return;                        
                    }

                case OperatorKind.Neg:
                    {
                        int i = (int)state.Pop();
                        state.Push(-i);
                        return;
                    }

                case OperatorKind.Not:
                    {
                        bool b = (bool)state.Pop();
                        state.Push(!b);
                        return;
                    }

                case OperatorKind.And:
                    {
                        bool b2 = (bool)state.Pop();
                        bool b1 = (bool)state.Pop();

                        state.Push(b1 && b2);
                        return;
                    }

                case OperatorKind.Or:
                    {
                        bool b2 = (bool)state.Pop();
                        bool b1 = (bool)state.Pop();

                        state.Push(b1 || b2);
                        return;
                    }

                case OperatorKind.Add:
                    {
                        int i2 = (int)state.Pop();
                        int i1 = (int)state.Pop();

                        state.Push(i1 + i2);
                        return;
                    }

                case OperatorKind.Sub:
                    {
                        int i2 = (int)state.Pop();
                        int i1 = (int)state.Pop();

                        state.Push(i1 - i2);
                        return;
                    }
            }

            throw new NotImplementedException();
        }
        
        // 해당 위치로 점프
        // JumpAddr
        public void Visit(Jump p)
        {
            // 노드는 들어오는 노드 여러개.. 나가는 노드 1개
            var i = state.CurFrame.Func.JumpTable[p.Point];
            state.Jump(i);
        }

        // Cond, JumpAddr
        public void Visit(IfJump p)
        {
            bool cond = (bool)state.Pop();

            if (cond)
            {
                var i = state.CurFrame.Func.JumpTable[p.Point];
                state.Jump(i);
            }
        }

        public void Visit(IfNotJump p)
        {
            bool cond = (bool)state.Pop();

            if (!cond)
            {
                var i = state.CurFrame.Func.JumpTable[p.Point];
                state.Jump(i);
            }
        }

        // arg1, ..., argN, MethodInfo
        public void Visit(StaticCall p)
        {
            FuncInfo func = p.FuncInfo;           

            // local 변수에 집어넣기..
            object[] args = new object[func.ArgCount];
            for (int t = func.ArgCount - 1; t >= 0; t--)
                args[t] = state.Pop();

            if (func.Extern)
            {
                // 외부 함수 
                Func<object[], object> externFunc;
                if (!externFuncMap.TryGetValue(p.FuncInfo.Name, out externFunc))
                    throw new NotImplementedException();

                var res = externFunc(args);

                if (p.FuncInfo.RetValCount == 1)
                    state.Push(res);
            }
            else
            {
                state.PushFrame(func.LocalCount, func);                

                for (int t = 0; t < args.Length; t++)
                    state.SetLocalValue(t, args[t]);
            }
        }

        // 함수가 끝났을 때, return 하는 개수만큼 제외한다
        public void Visit(Return p)
        {
            int retCount = state.CurFrame.Func.RetValCount;
            object[] retVals = new object[retCount];

            for (int t = retCount - 1; t >= 0; t--)
                retVals[t] = state.Pop();
            
            state.PopFrame();

            foreach( var retVal in retVals)
                state.Push(retVal);
        }

        // 로컬 변수의 값을 저장한다..
        public void Visit(StoreLocal p)
        {
            object val = state.Pop();
            state.SetLocalValue(p.Index, val);
        }

        // 로컬 변수의 값을 가져온다
        public void Visit(LoadLocal p)
        {
            state.Push(state.GetLocalValue(p.Index));
        }

        public void Visit(Yield p)
        {
            state.Pause = true;
        }

        public void Run(bool bIgnoreYield)
        {
            state.Pause = false;
            while (state.CurFrame.Func != null && state.Context.Point < state.CurFrame.Func.Commands.Count)
            {
                if (!bIgnoreYield && state.Pause) break;

                ICommand cmd = state.CurFrame.Func.Commands[state.Context.Point];
                state.Context.Point++;
                 
                // 실행
                cmd.Visit(this);
            }
        }

        public object Call(string name, params object[] args)
        {            
            var info = prog.GetFuncInfo(name);

            if (info == null)
                throw new InvalidOperationException();

            var prevCtx = state.Context;
            state.Context = new Context();

            // Call 만들기
            foreach (object arg in args)
                state.Push(arg);

            new StaticCall(info).Visit(this);
            
            Run(true);

            object retVal = state.Pop();
            state.Context = prevCtx;
            return retVal;
        }

        public void AddExternFunc(string name, Func<object[], object> func)
        {
            externFuncMap.Add(name, func);
        }
    }
}
