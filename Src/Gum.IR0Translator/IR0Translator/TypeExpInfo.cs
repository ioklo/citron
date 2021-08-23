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
        public abstract bool IsInternal(); // internal은 현재 컴파일 되고 있는 코드에 정의가 존재하는지 여부이다. 예를 들어 int는 internal이 아니다
    }

    record MTypeTypeExpInfo : TypeExpInfo
    {
        public M.Type Type { get; }
        public TypeExpInfoKind Kind { get; }
        bool bInternal;

        public MTypeTypeExpInfo(M.Type type, TypeExpInfoKind kind, bool bInternal)
        {
            this.Type = type;
            this.Kind = kind;
            this.bInternal = bInternal;
        }

        public override TypeExpInfoKind GetKind()
        {
            return Kind;
        }

        public override M.Type? GetMType()
        {
            return Type;
        }

        public override bool IsInternal()
        {
            return bInternal;
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

        public override bool IsInternal()
        {
            return false;
        }
    }
}
