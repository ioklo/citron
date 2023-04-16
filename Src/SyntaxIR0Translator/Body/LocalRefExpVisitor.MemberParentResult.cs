using Citron.Symbol;
using Citron.IR0;

namespace Citron.Analysis;

partial struct LocalRefExpVisitor
{
    interface IMemberParentResultVisitor<TResult>
    {
        TResult VisitLocation(MemberParentResult.Location result);
        TResult VisitBoxRef(MemberParentResult.BoxRef result);
        TResult VisitValue(MemberParentResult.Value result);

        TResult VisitNamespace(MemberParentResult.Namespace result);
        //TResult VisitLambdaMemberVar(MemberParentResult.LambdaMemberVar result);
        //TResult VisitLocalVar(MemberParentResult.LocalVar result);
        

        TResult VisitClass(MemberParentResult.Class result);
        // TResult VisitClassMemberVar(MemberParentResult.ClassMemberVar result);

        TResult VisitStruct(MemberParentResult.Struct result);
        // TResult VisitStructMemberVar(MemberParentResult.StructMemberVar result);

        TResult VisitThis(MemberParentResult.ThisVar result);
    }


    // 멤버 instance부분에 나올 수 있는 경우의 수들
    abstract record class MemberParentResult
    {
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IMemberParentResultVisitor<TResult>;

        // 메인이 되는 항목 셋, { 스택기반위치, Expression 밖에 인스턴스홀더가 존재하는 Box 참조, Local 참조 값으로 평가되는 값}
        public record class Location(Loc Loc, IType Type) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocation(this);
        }

        public record class BoxRef(Exp Exp, IType DerefType) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxRef(this);
        }

        public record class Value(Exp Exp) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitValue(this);
        }

        // 메인이 되는 항목에는 속하지 못하지만, Parent로써 역할을 하는 것들
        public record class Namespace(NamespaceSymbol Symbol) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
        }

        //public record class LambdaMemberVar(LambdaMemberVarSymbol MemberVar) : MemberParentResult
        //{
        //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
        //}

        //public record class LocalVar(IType Type, Name Name) : MemberParentResult
        //{
        //    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
        //}

        public record class Class(ClassSymbol Symbol) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
        }
        
        public record class Struct(StructSymbol Symbol) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
        }

        public record class ThisVar(IType Type) : MemberParentResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitThis(this);
        }
    }
}