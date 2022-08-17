using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;
using System.Diagnostics;

namespace Citron.Syntax
{
    public abstract record TypeExp : ISyntaxNode
    {
        TypeExpInfo? info;
        
        public TypeExpInfo Info
        {
            get 
            {
                if (info == null)
                    throw new NullReferenceException();
                
                return info; 
            } // valid after TypeExpEvaluationPhase
            set { info = value; }
        }
    }

    public record IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record NullableTypeExp(TypeExp InnerTypeExp) : TypeExp;
}