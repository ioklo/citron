using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.IR0Translator
{
    public abstract class ScriptTemplate
    {
        public ItemId Id { get; }

        public class Func : ScriptTemplate
        {
            public TypeValue? SeqElemTypeValue { get; }
            public bool bThisCall { get; }
            public int LocalVarCount { get; }
            public Stmt Body { get; }

            internal Func(ItemId funcId, TypeValue? seqElemTypeValue, bool bThisCall, int localVarCount, Stmt body)
                : base(funcId)
            {
                SeqElemTypeValue = seqElemTypeValue;
                this.bThisCall = bThisCall;
                LocalVarCount = localVarCount;
                Body = body;
            }
        }

        public class Enum : ScriptTemplate
        {
            public string DefaultElemName { get; }
            public ImmutableArray<(string Name, TypeValue TypeValue)> DefaultFields { get; }

            public Enum(ItemId enumId, string defaultElemName, IEnumerable<(string Name, TypeValue TypeValue)> defaultFields)
                : base(enumId)
            {
                DefaultElemName = defaultElemName;
                DefaultFields = defaultFields.ToImmutableArray();
            }
        }

        public ScriptTemplate(ItemId funcId)
        {
            Id = funcId;
        }

        public static Func MakeFunc(ItemId funcId, TypeValue? seqElemTypeValue, bool bThisCall, int localVarCount, Stmt body)
            => new Func(funcId, seqElemTypeValue, bThisCall, localVarCount, body);

        public static Enum MakeEnum(ItemId enumId, string defaultElemName, IEnumerable<(string Name, TypeValue TypeValue)> defaultFields)
            => new Enum(enumId, defaultElemName, defaultFields);
    }

}
