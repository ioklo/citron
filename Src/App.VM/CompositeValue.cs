using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;

namespace Gum.App.VM
{
    class CompositeValue : IValue
    {
        public IReadOnlyList<IValue> Fields { get; private set; }

        public CompositeValue(State state, IType type)
        {
            // TODO: 각 타입에 맞게 Field를 New 해 줍니다.

            var fields = new List<IValue>(type.Fields.Count);

            foreach (var field in type.Fields)
            {
                fields.Add(state.CreateValue(field));
            }

            Fields = fields;
        }

        public IType Type
        {
            get { throw new NotImplementedException(); }
        }

        public void CopyFrom(IValue value)
        {
            throw new NotImplementedException();
        }
    }
}
