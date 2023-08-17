using Citron.Collections;
using Citron.IR0;
using Citron.Symbol;

namespace Citron.Analysis;

interface IIntermediateRefExpVisitor<TResult>
{
    // statics
    TResult VisitNamespace(IntermediateRefExp.Namespace imRefExp);
    TResult VisitTypeVar(IntermediateRefExp.TypeVar imRefExp);
    TResult VisitClass(IntermediateRefExp.Class imRefExp);
    TResult VisitStruct(IntermediateRefExp.Struct imRefExp);
    TResult VisitEnum(IntermediateRefExp.Enum imRefExp);

    TResult VisitThis(IntermediateRefExp.ThisVar imRefExp);
    //TResult VisitLocalVar(IntermediateRefExp.LocalVar imRefExp);
    //TResult VisitLambdaMemberVar(IntermediateRefExp.LambdaMemberVar imRefExp);
    //TResult VisitClassMemberVar(IntermediateRefExp.ClassMemberVar imRefExp);
    //TResult VisitStructMemberVar(IntermediateRefExp.StructMemberVar imRefExp);
    //TResult VisitEnumElemMemberVar(IntermediateRefExp.EnumElemMemberVar imRefExp);

    //TResult VisitListIndexer(IntermediateRefExp.ListIndexer imRefExp);
    //TResult VisitLocalDeref(IntermediateRefExp.LocalDeref imRefExp);
    //TResult VisitBoxDeref(IntermediateRefExp.BoxDeref imRefExp);

    TResult VisitStaticRef(IntermediateRefExp.StaticRef imRefExp);
    TResult VisitBoxRef(IntermediateRefExp.BoxRef imRefExp);
    TResult VisitLocalRef(IntermediateRefExp.LocalRef imRefExp);

    TResult VisitDerefedBoxValue(IntermediateRefExp.DerefedBoxValue imRefExp);
    TResult VisitLocalValue(IntermediateRefExp.LocalValue imRefExp);
}

// &를 붙였을때의 행동이 좀 다르다
abstract record IntermediateRefExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IIntermediateRefExpVisitor<TResult>;

    public record class Namespace(NamespaceSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
    }
    
    public record class TypeVar(TypeVarType Type) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTypeVar(this);
    }

    public record class Class(ClassSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
    }
    
    public record class Struct(StructSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
    }
    
    public record class Enum(EnumSymbol Symbol) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }

    // 자체로는 invalid하지만 memberExp랑 결합되면 의미가 생기기때문에 정보를 갖고 있는다
    public record class ThisVar(IType Type) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
    }

    // exp로 사용할 수 있는
    //public record class LocalVar(IType Type, Name Name) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
    //}

    //public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
    //}

    //public record class ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
    //}

    //public record class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
    //}

    //public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ResolvedExp Instance) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
    //}

    //public record class ListIndexer(ResolvedExp Instance, R.Exp Index, IType ItemType) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
    //}

    //public record class LocalDeref(ResolvedExp Target) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
    //}

    //public record class BoxDeref(ResolvedExp Target) : IntermediateRefExp
    //{
    //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
    //}

    // &C.x, holder없이 주소가 살아있는
    public record class StaticRef(Loc Loc, IType LocType) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStaticRef(this);
    }

    public interface IBoxRefVisitor<TResult>
    {
        TResult VisitClassMember(BoxRef.ClassMember boxRef);
        TResult VisitStructIndirectMember(BoxRef.StructIndirectMember boxRef);
        TResult VisitStructMember(BoxRef.StructMember boxRef);
    }
    
    public abstract record class BoxRef : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxRef(this);
        public abstract IType GetTargetType();
        public abstract Loc MakeLoc();

        public abstract TResult AcceptBoxRef<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, IBoxRefVisitor<TResult>;
        
        // 홀더가 C로 시작하는 경우
        public record class ClassMember(Loc Loc, ClassMemberVarSymbol Symbol) : BoxRef
        {
            public override IType GetTargetType()
            {
                return Symbol.GetDeclType();
            }

            public override Loc MakeLoc()
            {
                return new ClassMemberLoc(Loc, Symbol);
            }

            public override TResult AcceptBoxRef<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMember(this);
        }

        // 홀더가 box* T로 시작하는 경우
        // (*pS).x => Exp는 pS를 말한다
        public record class StructIndirectMember(Exp Exp, StructMemberVarSymbol Symbol) : BoxRef
        {
            public override IType GetTargetType()
            {
                return Symbol.GetDeclType();
            }

            public override Loc MakeLoc()
            {
                return new StructMemberLoc(new BoxDerefLoc(Exp), Symbol);
            }

            public override TResult AcceptBoxRef<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructIndirectMember(this);
        }

        public record class StructMember(BoxRef Parent, StructMemberVarSymbol Symbol) : BoxRef
        {
            public override IType GetTargetType()
            {
                return Symbol.GetDeclType();
            }

            public override Loc MakeLoc()
            {
                var parentLoc = Parent.MakeLoc();
                return new StructMemberLoc(parentLoc, Symbol);
            }

            public override TResult AcceptBoxRef<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMember(this);
        }
    }
    
    public record class LocalRef(Loc Loc, IType LocType) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalRef(this);
    }

    // Value로 나오는 경우
    public record class LocalValue(Exp Exp) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalValue(this);
    }

    // handle
    // box S* pS;
    // box var* x = &(*pS).a; // x is box ptr (pS, ..)

    // *pS <- BoxValue, box S
    public record class DerefedBoxValue(Exp InnerExp) : IntermediateRefExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitDerefedBoxValue(this);
    }
}
