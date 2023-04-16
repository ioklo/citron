using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

interface IMemberResultVisitor<TResult>
{
    TResult VisitExp(MemberResult.Exp exp);
}

abstract record class MemberResult
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : IMemberResultVisitor<TResult>;

    public record class Exp(R.Exp RExp) : MemberResult
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitExp(this);        
    }
}

// S.MemberExp -> MemberResult
partial struct MemberExpMemberResultTranslator : IMemberResultVisitor<MemberResult>
{
    public static MemberResult Translate(S.MemberExp exp, ScopeContext context)
    {
        var parentVisitor = new ParentVisitor();
        var parentResult = exp.Accept<ParentVisitor, MemberResult>(ref parentVisitor);

        var visitor = new MemberExpMemberResultTranslator();
        return parentResult.Accept<MemberExpMemberResultTranslator, MemberResult>(ref visitor);

        //var parentResult = ExpMemberResultTranslator.Translate(memberExp.Parent, context, hintType: null);

        //var parentResult = ExpExpResultTranslator.Translate(memberExp.Parent, context, hintType: null);

        //// Loc으로 변환하기 전에 미리 처리할 경우들을 미리 처리한다
        //switch (parentResult)
        //{
        //    // VisitMemberTypeParent를 호출하는 경우,
        //    case ExpResult.Class classResult:
        //        return VisitMemberExpTypeParent(memberExp, classResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);

        //    case ExpResult.Struct structResult:
        //        return VisitMemberExpTypeParent(memberExp, structResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);

        //    case ExpResult.Enum enumResult:
        //        return VisitMemberExpTypeParent(memberExp, enumResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);

        //    // 에러
        //    case ExpResult.NotFound:
        //        context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp.Parent);
        //        throw new UnreachableException();

        //    case ExpResult.Error errorResult:
        //        HandleExpErrorResult(errorResult, context, memberExp.Parent);
        //        throw new UnreachableException();

        //    // "ns".id
        //    case ExpResult.Namespace:
        //        throw new NotImplementedException(); // NamespaceParent를 호출해야 한다

        //    // "T".id
        //    case ExpResult.TypeVar:
        //        context.AddFatalError(A2012_ResolveIdentifier_TypeVarCantHaveMember, memberExp);
        //        throw new UnreachableException();

        //    // Funcs류
        //    case ExpResult.GlobalFuncs:
        //    case ExpResult.ClassMemberFuncs:
        //    case ExpResult.StructMemberFuncs:
        //        context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
        //        break;

        //    // 'Second.x'
        //    case ExpResult.EnumElem:
        //        context.AddFatalError(A2009_ResolveIdentifier_EnumElemCantHaveMember, memberExp);
        //        break;

        //    // location으로 변환해야 할 것들
        //    case ExpResult.ThisVar: // "this".id
        //    case ExpResult.LocalVar:// "l".id
        //    case ExpResult.LambdaMemberVar:// "x".id 
        //    case ExpResult.ClassMemberVar:
        //    case ExpResult.StructMemberVar:
        //    case ExpResult.EnumElemMemberVar:
        //    case ExpResult.IR0Exp:
        //    case ExpResult.IR0Loc:
        //        {
        //            var parentLocResult = parentResult.MakeIR0Loc(bWrapExpAsLoc: true);
        //            // loc으로 변환 가능했다면
        //            if (parentLocResult == null)
        //                throw new UnreachableException();

        //            var (parentLoc, parentType) = parentLocResult.Value;
        //            return VisitMemberExpLocParent(memberExp, parentLoc, parentType);
        //        }
        //}

        //throw new UnreachableException();
    }


}
