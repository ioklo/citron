using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;
using System.Diagnostics;

namespace Citron.Syntax
{
    public abstract record class TypeExp : ISyntaxNode
    {
        enum InitializeState
        {
            BeforeInitType,
            AfterInitType
        }

        object? type; // IType, 하위 dependency이기 때문에, object 레퍼런스로 가리키다가 캐스팅한다
        InitializeState initState;

        public void InitType(object type)
        {
            Debug.Assert(initState == InitializeState.BeforeInitType);
            this.type = type;

            this.initState = InitializeState.AfterInitType;
        }

        public new object GetType()
        {
            Debug.Assert(InitializeState.BeforeInitType < initState);
            return type!;
        }
    }

    public record class IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record class MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record class NullableTypeExp(TypeExp InnerTypeExp) : TypeExp;
}