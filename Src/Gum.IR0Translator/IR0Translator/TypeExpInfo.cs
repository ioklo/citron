using Pretune;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    enum TypeExpInfoKind
    {
        Class,
        Struct,
        Interface,
        Enum,
        EnumElem,
        Var,
        Void,
        TypeVar,
    }

    // X<int>.Y<T>, closed
    abstract record TypeExpInfo
    {
        public abstract TypeExpInfoKind GetKind();
        public abstract M.Type? GetMType();
    }

    record MTypeTypeExpInfo(M.Type Type, TypeExpInfoKind Kind) : TypeExpInfo
    {
        public override TypeExpInfoKind GetKind()
        {
            return Kind;
        }

        public override M.Type? GetMType()
        {
            return Type;
        }
    }

    record VarTypeExpInfo : TypeExpInfo
    {
        public static readonly VarTypeExpInfo Instance = new VarTypeExpInfo();
        VarTypeExpInfo() { }

        public override TypeExpInfoKind GetKind()
        {
            return TypeExpInfoKind.Var;
        }

        public override M.Type? GetMType()
        {
            return null;
        }
    }
}
