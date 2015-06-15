using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;
using System.Collections.Immutable;
using Gum.Core.Runtime;

namespace Gum.App.VM
{
    public class State
    {
        private Stack<Frame> frameStack = new Stack<Frame>();
        private Frame curFrame = null;
        private List<IValue> globalValues = new List<IValue>();

        public int ReturnDest { get { return curFrame.ReturnDest;  } }

        public State(IEnumerable<IType> localTypes)
        {
            Block emptyBlock = new Block();

            Function func = new Function("<init>", GlobalDomain.VoidType, Enumerable.Empty<IType>(), localTypes, Enumerable.Repeat(emptyBlock, 1));
            curFrame = new Frame(this, -1, func);
        }
                
        public void SetLocalValue(int regIndex, IValue value)
        {
            curFrame.SetLocalValue(regIndex, value);            
        }

        public T GetLocalValue<T>(int regIndex) where T : class, IValue
        {
            return curFrame.GetLocalValue<T>(regIndex);
        }

        public T GetGlobalValue<T>(int globalIndex) where T : class, IValue
        {
            return globalValues[globalIndex] as T;
        }
        
        public void SetValue(RefValue refValue, IValue value)
        {
            // refValue가 가리키는 곳에 value값을 복사합니다.
            refValue.Value.CopyFrom(value);
        }        

        public T GetValue<T>(RefValue refValue) where T : class, IValue
        {
            return refValue.Value as T;
        }
        
        public void PushFrame(int retIndex, Function func)
        {
            frameStack.Push(curFrame);
            curFrame = new Frame(this, retIndex, func);
        }

        public void PopFrame()
        {
            curFrame = frameStack.Pop();
        }
        
        public void SetExecutionPoint(int block, int index)
        {
            curFrame.SetExecutionPoint(block, index);
        }

        public IValue CreateValue(IType type)
        {
            if (type is RefType)
                return new RefValue(null);

            if (type == GlobalDomain.IntType)
                return new IntValue(0);

            if (type == GlobalDomain.BoolType)
                return new BoolValue(false);

            if (type is FunctionType)
                return new FuncValue(null);

            throw new NotImplementedException();
        }
    }
}
