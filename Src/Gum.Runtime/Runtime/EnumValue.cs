using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gum.Runtime
{
    public class EnumValue : Value
    {
        public string ElemName { get; private set; }
        private Dictionary<string, Value> values;

        public EnumValue()
        {
            ElemName = string.Empty;
            values = new Dictionary<string, Value>();
        }
        
        public EnumValue(string elemName, IEnumerable<(string Name, Value Value)> memberValues)
        {
            ElemName = elemName;

            values = new Dictionary<string, Value>();
            foreach (var memberValue in memberValues)
                values.Add(memberValue.Name, memberValue.Value);
        }        
        
        public override Value MakeCopy()
        {
            var copiedMembers = values.Select(e => (e.Key, e.Value));
            return new EnumValue(ElemName, copiedMembers);
        }

        public override void SetValue(Value fromValue)
        {
            EnumValue enumValue = (EnumValue)fromValue;
            SetValue(enumValue.ElemName, enumValue.values.Select(tv => (tv.Key, tv.Value)));
        }

        public void SetValue(string elemName, IEnumerable<(string Name, Value Value)> memberValues)
        {
            if (ElemName != elemName)
            {
                ElemName = elemName;
                values.Clear();
                foreach (var memberValue in memberValues)
                    values[memberValue.Name] = memberValue.Value.MakeCopy();
            }
            else
            {
                foreach (var memberValue in memberValues)
                    values[memberValue.Name].SetValue(memberValue.Value);
            }
        }

        public Value GetValue(string memberName)
        {
            return values[memberName];
        }
    }
    
}

