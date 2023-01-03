using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using Citron.Module;

namespace Citron.Symbol
{
    // value
    [AutoConstructor]
    public partial struct FuncParameter
    {
        public FuncParameterKind Kind { get; }
        public IType Type { get; }
        public Name Name { get; }        

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(Kind, appliedType, Name);
        }
    }

    public static class FuncParameterExtensions
    {   
    }
}