using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;
using System.Collections.Immutable;

namespace Gum.App.VM
{
    // 현재 함수에서의 컨텍스트  (실제 인스턴스)
    public class Frame
    {
        private Function func;

        private int returnDest;
        private List<IValue> locals;

        private Block curBlock;
        private int curIndex;

        public int ReturnDest { get { return returnDest; } }

        public Frame(State state, int returnRegIndex, Function func)
        {
            this.returnDest = returnRegIndex;
            this.func = func;
            this.locals = new List<IValue>(func.ArgTypes.Count + func.LocalTypes.Count);
            this.curBlock = func.StartBlock;
            this.curIndex = 0;

            for (int t = 0; t < func.ArgTypes.Count; t++)
            {
                var value = state.CreateValue(func.ArgTypes[t]);
                this.locals.Add(value);
            }

            for (int t = 0; t < func.LocalTypes.Count; t++)
            {
                var value = state.CreateValue(func.LocalTypes[t]);
                this.locals.Add(value);
            }
        
        }

        public void SetLocalValue(int localIndex, IValue val)
        {
            locals[localIndex].CopyFrom(val);
        }

        public T GetLocalValue<T>(int localIndex) where T : class, IValue
        {
            return locals[localIndex] as T;
        }

        public void SetExecutionPoint(int block, int index)
        {
            curBlock = func.Blocks[block];
            curIndex = index;
        }
    }
}
