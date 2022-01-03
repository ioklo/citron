using System.Collections.Generic;
using System.Linq;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public enum TypeExpInfoKind
    {
        Class,
        Struct,
        Interface,
        Enum,
        EnumElem,
        Var,
        Void,
        TypeVar,
        Nullable,
    }

    // X<int>.Y<T>, closed
    public abstract record TypeExpInfo
    {
        public abstract TypeExpInfoKind GetKind();
        public abstract M.TypeId? GetMType();
        public abstract bool IsInternal();   // internal은 현재 컴파일 되고 있는 코드에 정의가 존재하는지 여부이다. 예를 들어 int는 internal이 아니다
    }

    public record MTypeTypeExpInfo : TypeExpInfo
    {
        public M.TypeId Type { get; }
        public TypeExpInfoKind Kind { get; }
        bool bInternal;

        public MTypeTypeExpInfo(M.TypeId type, TypeExpInfoKind kind, bool bInternal)
        {
            this.Type = type;
            this.Kind = kind;
            this.bInternal = bInternal;
        }

        public override TypeExpInfoKind GetKind()
        {
            return Kind;
        }

        public override M.TypeId? GetMType()
        {
            return Type;
        }

        public override bool IsInternal()
        {
            return bInternal;
        }        
    }

    public record VarTypeExpInfo : TypeExpInfo
    {
        public static readonly VarTypeExpInfo Instance = new VarTypeExpInfo();
        VarTypeExpInfo() { }

        public override TypeExpInfoKind GetKind()
        {
            return TypeExpInfoKind.Var;
        }

        public override M.TypeId? GetMType()
        {
            return null;
        }

        public override bool IsInternal()
        {
            return false;
        }
    }
}
