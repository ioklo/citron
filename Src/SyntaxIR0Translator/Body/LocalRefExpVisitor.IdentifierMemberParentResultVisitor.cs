using Citron.Symbol;
using Citron.IR0;

namespace Citron.Analysis;


partial struct LocalRefExpVisitor
{   
    // S.Exp -> MemberParentResult
    // var& x = 'id'.x;
    // id가 local일때, C, S의 정적 멤버변수 일때, struct의 this일때(S& 타입), lambda member var일때 가능
    struct IdentifierMemberParentResultVisitor : IIdentifierResultVisitor<MemberParentResult>
    {
        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitNamespace(IdentifierResult.Namespace result)
        {
            // NS.C.id 가능
            return new MemberParentResult.Namespace(result.Symbol);
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitLambdaMemberVar(IdentifierResult.LambdaMemberVar result)
        {
            // () => { var& x = s.id; }, 가능

            // Lambda 인스턴스는 스택에 존재한다고 본다 (TODO: box lambda instance는 나중에 따로 처리)
            return new MemberParentResult.Location(new LambdaMemberVarLoc(result.MemberVar));
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitClass(IdentifierResult.Class result)
        {
            // var& x = C.id, 가능
            return new MemberParentResult.Class(result.Symbol);
        }
        
        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitThisClassMemberVar(IdentifierResult.ThisClassMemberVar result)
        {
            // var& x = C.x.id; 가능
            // var& x = c.x.id; 가능

            // static은 홀더가 없어도 괜찮다, 
            if (result.Symbol.IsStatic())
            {
                return new MemberParentResult.Location(new ClassMemberLoc(null, result.Symbol));
            }
            else
            {
                // this는 expression 밖에 홀더가 있는 것으로 간주한다
                return new MemberParentResult.BoxRef(new ClassMemberVarBoxRefExp(new ThisLoc(), result.Symbol));
            }
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitLocalVar(IdentifierResult.LocalVar result)
        {
            // var& x = 's'.x;

            // s가 지역참조 인 경우 s 로 할 것이냐, *s로 할 것이냐 => *s가 맞는거 같다
            if (result.Type is LocalRefType localRefType)
            {
                return new MemberParentResult.Location(new LocalDerefLoc(new LocalVarLoc(result.Name)), localRefType.InnerType);
            }
            else
            {
                return new MemberParentResult.Location(new LocalVarLoc(result.Name), result.Type);
            }
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitStruct(IdentifierResult.Struct result)
        {
            // 가능
            return new MemberParentResult.Struct(result.Symbol);
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitThisStructMemberVar(IdentifierResult.ThisStructMemberVar result)
        {            
            if (result.Symbol.IsStatic())
            {
                return new MemberParentResult.Location(new StructMemberLoc(null, result.Symbol));
            }
            else
            {   
                return new MemberParentResult.Location(new StructMemberLoc(new ThisLoc(), result.Symbol));
            }
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitThis(IdentifierResult.ThisVar result)
        {
            // 클래스 변수는 바로 box 참조로 만들 Exp가 없기 때문에 (꼭 멤버변수가 있어야 한다)
            // 지금은 ThisVar로 두고, parent.id 결합시점에 Exp로 만든다

            // var& x = this.x;, 가능
            return new MemberParentResult.ThisVar(result.Type);
        }

        // 아래는 불가능
        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitEnum(IdentifierResult.Enum result)
        {
            // var& x = E.x; 불가능
            throw new System.NotImplementedException();
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitEnumElem(IdentifierResult.EnumElem result)
        {
            // var& x = E.First.x, 불가능
            throw new System.NotImplementedException();
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitGlobalFuncs(IdentifierResult.GlobalFuncs result)
        {
            // var& x = F.x, 에러.
            throw new System.NotImplementedException();
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitThisClassMemberFuncs(IdentifierResult.ThisClassMemberFuncs result)
        {
            // var& x = C.F.id; 불가능
            throw new System.NotImplementedException();
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitThisStructMemberFuncs(IdentifierResult.ThisStructMemberFuncs result)
        {
            // 불가능
            throw new System.NotImplementedException();
        }

        MemberParentResult IIdentifierResultVisitor<MemberParentResult>.VisitTypeVar(IdentifierResult.TypeVar result)
        {
            // var& x = T.id; 불가능
            throw new System.NotImplementedException();
        }
    }
}