using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;
using Gum.Core.Runtime;
using Gum.Core.IL.Commands;

namespace Gum.App.VM
{
    // 인터프리터
    public class Interpreter : ICommandVisitor
    {   
        // 내부에 스테이트를 갖고 있고, 
        State state;

        // 미리 갖고 있는 ExternalMap을 갖고 있습니다
        Dictionary<string, Func<object[], object>> externFuncMap = new Dictionary<string, Func<object[], object>>();

        // 그리고 프로그램
        public Interpreter(IEnumerable<IType> locals)
        {
            state = new State(locals);
        }

        // 1. GlobalRef
        public void Visit(GlobalRef globalRef)
        {
            // 현 상태에서 globalRef를 가져옵니다.
            IValue globalValue = state.GetGlobalValue<IValue>(globalRef.Index);
            RefValue refValue = state.GetLocalValue<RefValue>(globalRef.DestReg);

            refValue.Set(globalValue);
        }

        // 2. LocalRef
        public void Visit(LocalRef localRef)
        {
            // 현 상태에서 localRef를 가져옵니다.
            IValue localValue = state.GetLocalValue<IValue>(localRef.Index);
            RefValue refValue = state.GetLocalValue<RefValue>(localRef.DestReg);

            refValue.Set(localValue);
        }

        // 3. FieldRef
        public void Visit(FieldRef fieldRef)
        {
            RefValue srcRef = state.GetLocalValue<RefValue>(fieldRef.SrcRefReg);
            CompositeValue compValue = srcRef.Value as CompositeValue;
            IValue value = compValue.Fields[fieldRef.Index];

            RefValue refValue = state.GetLocalValue<RefValue>(fieldRef.DestReg);
            refValue.Set(value);            
        }        

        // 4. New
        public void Visit(New newCmd)
        {
            RefValue typeRefValue = state.GetLocalValue<RefValue>(newCmd.Type);
            TypeValue typeValue = (TypeValue)typeRefValue.Value;

            var compValue = new CompositeValue(state, typeValue.Value);

            RefValue refValue = state.GetLocalValue<RefValue>(newCmd.DestReg);
            refValue.Set(compValue);
        }

        // 5. Load
        public void Visit(Load load)
        {
            IValue val = state.GetLocalValue<IValue>(load.Dest);
            RefValue srcRef = state.GetLocalValue<RefValue>(load.SrcRef);

            val.CopyFrom(srcRef.Value);
        }
        
        // 6. Store 
        public void Visit(Store store)
        {
            RefValue destRef = state.GetLocalValue<RefValue>(store.DestRef);
            IValue val = state.GetLocalValue<IValue>(store.Src);

            destRef.Value.CopyFrom(val);
        }

        // 7. Move 
        public void Visit(Move move)
        {
            IValue val = state.GetLocalValue<IValue>(move.Dest);
            val.CopyFrom(move.Value);
        }

        public void Visit(MoveReg moveReg)
        {
            IValue srcVal = state.GetLocalValue<IValue>(moveReg.Src);
            IValue destVal = state.GetLocalValue<IValue>(moveReg.Dest);
            destVal.CopyFrom(srcVal);
        }
        
        // 8. Jump, 현재 실행 블럭을 옮깁니다
        public void Visit(Jump jump)
        {
            state.SetExecutionPoint(jump.Block, 0);
        }

        // 9. CondJump
        public void Visit(IfNotJump condJump)
        {
            BoolValue boolValue = state.GetLocalValue<BoolValue>(condJump.Cond);
            if (!boolValue.Value)
                state.SetExecutionPoint(condJump.Block, 0);
        }
        
        // 10. StaticCall staticCallCmd);
        // arg1, ..., argN, MethodInfo
        public void Visit(StaticCall staticCall)
        {
            // 1. 함수 가져오기
            var funcValue = state.GetLocalValue<FuncValue>(staticCall.Func);

            // 2. 인자 가져오기
            IValue[] values = new IValue[funcValue.Value.ArgTypes.Count];

            for (int t = 0; t < staticCall.Args.Count; t++)
                values[t] = state.GetLocalValue<IValue>(staticCall.Args[t]);

            // 내부 함수라면
            if (funcValue.Value is Function)
            {   
                // 3. 새 프레임을 만들고 인자를 복사하기
                state.PushFrame(staticCall.Ret, (Function)funcValue.Value);

                for (int t = 0; t < values.Length; t++)
                    state.SetLocalValue(t, values[t]);
            }
            else if (funcValue.Value is ExternFunction)
            {
                ExternFunction externFunc = funcValue.Value as ExternFunction;
                var res = externFunc.Instance(values);

                if (externFunc.RetType != GlobalDomain.VoidType)
                    state.SetLocalValue(staticCall.Ret, (IValue)res);
            }           
        }

        // VirtualCall
        public void Visit(VirtualCall virtualCall)
        {
            throw new NotImplementedException();
        }

        // 12. Return
        public void Visit(Return returnCmd)
        {
            int retDest= state.ReturnDest; // PopFrame을 하면, ReturnDest가 달라지므로 미리 저장해야 합니다
            IValue retValue = state.GetLocalValue<IValue>(returnCmd.Value);

            state.PopFrame();
            state.SetLocalValue(retDest, retValue);
        }

        //public object Call(Domain domain, string name, params object[] args)
        //{
        //    IValue value;
        //    if (!domain.TryGetValue(name, out value))
        //        throw new InvalidOperationException();

        //    var info = value as IFunction;

        //    if (info == null)
        //        throw new InvalidOperationException();

        //    var prevCtx = state.Context;
        //    state.Context = new Context();

        //    // Call 만들기
        //    foreach (object arg in args)
        //        state.Push(arg);

        //    new StaticCall(info).Visit(this);
            
        //    Run(true);

        //    object retVal = state.Pop();
        //    state.Context = prevCtx;
        //    return retVal;
        //}

        public void AddExternFunc(string name, Func<object[], object> func)
        {
            externFuncMap.Add(name, func);
        }






    }
}
