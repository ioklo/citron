using Citron.Infra;
using Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;
using System;
using Citron.Symbol;

namespace Citron.Analysis;

abstract record class LocalRefResult
{
    public record class BoxRef(Exp Exp) : LocalRefResult;
    public record class Location(Loc Loc) : LocalRefResult;
    public record class Value(Exp Exp) : LocalRefResult;
}

// ClassMemberVar, StructMemberVar의 Instance자리에 있던 ExpResult를 LocalRefResult로 바꾸는 역할을 한다
// ClassMemberVar, StructMemberVar의 Instance자리에는 무엇이 올수 있는가
// valueExp, loc만 올 수 있다
// Class, Struct, TypeVar등 올 수 없다
// ExpResult -> LocalRefResult
struct ResolvedExpLocalRefResultTranslator : IResolvedExpVisitor<LocalRefResult>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static LocalRefResult Translate(ResolvedExp reExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var translator = new ResolvedExpLocalRefResultTranslator() { context = context };
        return reExp.Accept<ResolvedExpLocalRefResultTranslator, LocalRefResult>(ref translator);
    }

    LocalRefResult Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return Error();
    }

    // box S& s; ... var& x = s.a; 
    // box S* s; ... var* x = &((*s).a); // 이건 ResolvedExp에서 해야 한다
    LocalRefResult HandleLocalLoc(Loc loc, IType type)
    {
        if (type is LocalRefType)
            return new LocalRefResult.Location(new LocalDerefLoc(loc));
        else if (type is BoxRefType) 
            return new LocalRefResult.BoxRef()

    }
    
    // F().x
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitIR0Exp(ResolvedExp.IR0Exp result)
    {
        // 일반 exp
        return new LocalRefResult.Value(result.Exp);
    }

    #region Location or Value

    // S s = 0;
    // var f = () => { var& x = s.a; };
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar result)
    {
        return new LocalRefResult.Location(new LambdaMemberVarLoc(result.Symbol));
    }

    // l[0].a
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitListIndexer(ResolvedExp.ListIndexer listIndexer)
    {
        return Fatal(A3005_LocalRef_ListItemIsNotReferrable);
    }

    // x
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitLocalVar(ResolvedExp.LocalVar result)
    {
        return new LocalRefResult.Location(new LocalVarLoc(result.Name));
    }

    // c.x
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitClassMemberVar(ResolvedExp.ClassMemberVar result)
    {
        if (result.HasExplicitInstance)
        {
            if (result.ExplicitInstance != null)
            {
                // class이기 때문에, boxRef는 여기서부터 생성한다. 그 위로는 보통 MemberExpression을 따른다
                var instLoc = ResolvedExpIR0LocTranslator.Translate(result.ExplicitInstance, context, bWrapExpAsLoc: false, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
                return new LocalRefResult.BoxRef(new ClassMemberVarBoxRefExp(instLoc, result.Symbol));
            }
        }
    }

    // e.x
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar result)
    {
        return Fatal(A3004_LocalRef_EnumIsNotReferrable);
    }

    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitStructMemberVar(ResolvedExp.StructMemberVar result)
    {
        if (result.HasExplicitInstance)
        {
            if (result.ExplicitInstance != null)
            {
                var instanceResult = ResolvedExpLocalRefResultTranslator.Translate(result.ExplicitInstance, context, nodeForErrorReport);

                switch (instanceResult)
                {
                    // 한번 box 참조가 되었으면 계속 
                    case LocalRefResult.BoxRef boxResult:
                        return new LocalRefResult.BoxRef(new StructMemberVarBoxRefExp(boxResult.Exp, result.Symbol));

                    case LocalRefResult.Location locResult:
                        return new LocalRefResult.Location(new StructMemberLoc(locResult.Loc, result.Symbol));

                    case LocalRefResult.Value valueResult:
                        return new LocalRefResult.Value(valueResult.Exp)
                }
            }
        }        
    }

    // 지금은 중간과정임을 생각해야 한다
    LocalRefResult IResolvedExpVisitor<LocalRefResult>.VisitThis(ResolvedExp.ThisVar result)
    {
        throw new NotImplementedException();
    }

    #endregion Location or Value

}

// nested 안되는 케이스를 모두 작성한다
// ExpResult -> R.Exp (LocalRef)
struct ExpResultLocalRefTranslator : IExpResultVisitor<R.Exp>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    Exp FatalError(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return Error();
    }

    Exp IExpResultVisitor<Exp>.VisitError(ResolvedExp.Error result)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitNotFound(ResolvedExp.NotFound result)
    {
        throw new System.NotImplementedException();
    }

    // ExpResult의 최종형태로 나올수 없는 것들 모음, 타입, Namespace
    #region Unreachable

    // var& x = NS;
    Exp IExpResultVisitor<Exp>.VisitNamespace(ResolvedExp.Namespace result)
    {
        throw new UnreachableException();
    }

    // var& x = C;
    Exp IExpResultVisitor<Exp>.VisitClass(ResolvedExp.Class result)
    {
        throw new UnreachableException();
    }

    // var& x = S;
    Exp IExpResultVisitor<Exp>.VisitStruct(ResolvedExp.Struct result)
    {
        throw new System.NotImplementedException();
    }

    // var& x = E;
    Exp IExpResultVisitor<Exp>.VisitEnum(ResolvedExp.Enum result)
    {   
        throw new UnreachableException();
    }

    // var& x = T;
    Exp IExpResultVisitor<Exp>.VisitTypeVar(ResolvedExp.TypeVar result)
    {
        throw new UnreachableException();
    }

    #endregion Unreachable

    // ExpResult의 최종형태로 나올 수는 있지만, 위치를 얻어낼 수는 없다
    #region Non-Location 

    Exp IExpResultVisitor<Exp>.VisitGlobalFuncs(ResolvedExp.GlobalFuncs result)
    {
        return FatalError(A3001_LocalRef_ExpressionIsNotLocation);
    }

    // var& x = C.F;
    Exp IExpResultVisitor<Exp>.VisitClassMemberFuncs(ResolvedExp.ClassMemberFuncs result)
    {
        return FatalError(A3001_LocalRef_ExpressionIsNotLocation);
    }

    Exp IExpResultVisitor<Exp>.VisitStructMemberFuncs(ResolvedExp.StructMemberFuncs result)
    {
        return FatalError(A3001_LocalRef_ExpressionIsNotLocation);
    }

    // var& x = E.First
    Exp IExpResultVisitor<Exp>.VisitEnumElem(ResolvedExp.EnumElem result)
    {
        return FatalError(A3001_LocalRef_ExpressionIsNotLocation);
    }

    #endregion Non-Location

    #region Value

    // value로 계산되는 경우, value의 최종값이 레퍼런스 인 경우에만 가능하다
    // var& x = f();
    Exp IExpResultVisitor<Exp>.VisitIR0Exp(ResolvedExp.IR0Exp result)
    {
        // box var&의 경우, 
        // 일단 값 자체가 reference인지 확인한다.
        var type = result.Exp.GetExpType();

        // box int& F() { ... }
        // var& x = F(); // 에러, F()의 리턴값은 언제 사라질지 모른다, 따라서 box ref에서 변환 불가능
        if (type is LocalRefType) 
        {
            // 타입 체킹은 언제하는가, 지금은 상대 타입이 var&일수도 있고, int&일수도 있으니 불가능하다.
            return result.Exp;
        }
        else
        {
            return FatalError(A3006_LocalRef_ExpressionTypeShouldBeLocalRef);
        }
    }

    #endregion Value

    #region Location

    static Exp HandleLoc(Loc loc, IType type)
    {
        if (type is LocalRefType)
        {
            // 로컬 참조 타입이면, load
            // var i = 0;
            // var& x = i;
            // var& y = x; // <- 
            return new LoadExp(loc, type);
        }
        else
        {
            // 아니라면 
            // var i = 0;
            // var& x = i; // <- 

            return new LocalRefExp(loc, type);
        }
    }
    
    Exp IExpResultVisitor<Exp>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar result)
    {
        // var a = 3;
        // var f = () => { var& x = a; }
        return HandleLoc(new LambdaMemberVarLoc(result.Symbol), result.Symbol.GetDeclType());
    }

    Exp IExpResultVisitor<Exp>.VisitLocalVar(ResolvedExp.LocalVar result)
    {
        return HandleLoc(new LocalVarLoc(result.Name), result.Type);
    }

    Exp IExpResultVisitor<Exp>.VisitThis(ResolvedExp.ThisVar result)
    {
        return HandleLoc(new ThisLoc(), result.Type);
    }

    #endregion Location

    // 재귀적으로 만들어야 하는 것들
    #region Members

    // var& x = C.x;
    Exp IExpResultVisitor<Exp>.VisitClassMemberVar(ResolvedExp.ClassMemberVar result)
    {

        // 재귀적으로 해야 한다
        throw new NotImplementedException();

        // 일단 값 자체가 reference인지 확인한다.
        var type = result.Symbol.GetDeclType();
        if (type is LocalRefType || type is BoxRefType)
        {
            var locAndType = ResolvedExpIR0LocTranslator.Translate(result, context, bWrapExpAsLoc: false);
            if (locAndType == null)
                throw new NotImplementedException();

            return new LoadExp(locAndType.Value.Loc, type);
        }

        // 값 자체가 reference가 아니라면, 변환할 수 있는지 본다

        // 1. static이면 loc에서 바로 변환 가능, holder가 필요하지 않다
        if (result.Symbol.IsStatic())
        {
            var locAndType = ResolvedExpIR0LocTranslator.Translate(result, context, bWrapExpAsLoc: false);
            if (locAndType == null)
                throw new NotImplementedException();

            return new LocalRefExp(locAndType.Value.Loc, locAndType.Value.Type);
        }
        else
        {
            // instance를 알고 있어야 한다
            Loc instance;

            if (result.HasExplicitInstance)
            {
                // var& x = c.a; (this.a)
                Debug.Assert(result.ExplicitInstance != null);

                var locAndType = ResolvedExpIR0LocTranslator.Translate(result.ExplicitInstance, context, bWrapExpAsLoc: true);
                if (locAndType == null)
                    return FatalError(A3003_LocalRef_MemberParentIsNotLocation);

                instance = locAndType.Value.Loc;
            }
            else
            {
                // implicit instance
                // var& x = a; (this.a)
                instance = new ThisLoc();
            }

            return new ClassMemberVarBoxRefExp(instance, result.Symbol);
        }
    }

    Exp IExpResultVisitor<Exp>.VisitStructMemberVar(ResolvedExp.StructMemberVar result)
    {
        // 그냥 reference라면 Load
        var type = result.Symbol.GetDeclType()
        if (type is LocalRefType || type is BoxRefType)
        {


        }


    }

    // var& x = e.x;
    Exp IExpResultVisitor<Exp>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar result)
    {
        return FatalError(A3004_LocalRef_EnumIsNotReferrable);
    }

    // var& x = l[2];
    Exp IExpResultVisitor<Exp>.VisitListIndexer(ResolvedExp.ListIndexer listIndexer)
    {
        return FatalError(A3005_LocalRef_ListItemIsNotReferrable);
    }

    #endregion Members
}
