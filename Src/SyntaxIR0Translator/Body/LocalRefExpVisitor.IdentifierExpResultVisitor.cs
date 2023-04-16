using System;
using SExp = Citron.Syntax.Exp;
using Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.Analysis;

partial struct LocalRefExpVisitor
{
    //void Main()
    //{
    //    switch (result)
    //    {
    //        case ExpResult.NotFound:
    //            break;

    //        case ExpResult.Error:


    //        // Valid
    //        #region TopLevel
    //        case ExpResult.Namespace:
    //            throw new NotImplementedException(); // Namespace를 로컬 레퍼런스롤 만들 수 없습니다

    //        case ExpResult.GlobalFuncs:
    //            throw new NotImplementedException(); // GlobalFunc 군을 레퍼런스로 만들 수 없습니다
    //        #endregion

    //        #region Body
    //        case ExpResult.TypeVar:
    //            throw new NotImplementedException(); // TypeVar를 로컬 레퍼런스로 만들 수 없습니다

    //        // this는 class라면,
    //        case ExpResult.ThisVar thisVarResult:
    //            return (new R.ThisLoc(), thisVarResult.Type);

    //        // Body에 직접 있는 로컬 
    //        case ExpResult.LocalVar localVarResult:
    //            return (new R.LocalVarLoc(localVarResult.Name), localVarResult.Type);

    //        // Lambda안에 있는 외부 참조
    //        case ExpResult.LambdaMemberVar lambdaMemberVarResult:
    //            var lambdaMemberVar = new R.LambdaMemberVarLoc(lambdaMemberVarResult.MemberVar);
    //            return (lambdaMemberVar, lambdaMemberVarResult.MemberVar.GetDeclType());

    //        #endregion

    //        #region Class
    //        case ExpResult.Class: return null;

    //        // loc으로 만들 수 있는가
    //        // var x = F; // F는 여러개인데;
    //        case ExpResult.ClassMemberFuncs: throw new NotImplementedException();

    //        case ExpResult.ClassMemberVar classMemberVarResult:

    //            if (classMemberVarResult.HasExplicitInstance) // c.x, C.x 둘다 해당
    //            {
    //                return (new R.ClassMemberLoc(classMemberVarResult.ExplicitInstance, classMemberVarResult.Symbol), classMemberVarResult.Symbol.GetDeclType());
    //            }
    //            else // x, x (static) 둘다 해당
    //            {
    //                var instanceLoc = classMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
    //                return (new R.ClassMemberLoc(instanceLoc, classMemberVarResult.Symbol), classMemberVarResult.Symbol.GetDeclType());
    //            }

    //        #endregion

    //        #region Struct
    //        case ExpResult.Struct: return null;
    //        case ExpResult.StructMemberFuncs: return null;
    //        case ExpResult.StructMemberVar structMemberVarResult:
    //            if (structMemberVarResult.HasExplicitInstance) // c.x, C.x 둘다 해당
    //            {
    //                return (new R.StructMemberLoc(structMemberVarResult.ExplicitInstance, structMemberVarResult.Symbol), structMemberVarResult.Symbol.GetDeclType());
    //            }
    //            else // x, x (static) 둘다 해당
    //            {
    //                var instanceLoc = structMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
    //                return (new R.StructMemberLoc(instanceLoc, structMemberVarResult.Symbol), structMemberVarResult.Symbol.GetDeclType());
    //            }

    //        #endregion

    //        #region Enum
    //        case ExpResult.Enum: return null;

    //        // First
    //        case ExpResult.EnumElem: return null;

    //        // 
    //        case ExpResult.EnumElemMemberVar: return null;
    //        #endregion

    //        case ExpResult.IR0Exp rexp:
    //            if (bWrapExpAsLoc)
    //                return (new R.TempLoc(rexp.Exp), rexp.Exp.GetExpType());
    //            else
    //                return null;

    //        case ExpResult.IR0Loc rloc:
    //            return (rloc.Loc, rloc.Type);

    //        default:
    //            throw new UnreachableException();
    //    }



    //    if (locResult == null)
    //        context.AddFatalError(A3001_Ref_ExpressionIsNotLocation, exp);

    //    var (loc, locType) = locResult.Value;
    //    return new R.LocalRefExp(loc, locType);


    //    var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
    //    var expResult = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
    //}

    struct IdentifierExpResultVisitor : IIdentifierResultVisitor<Exp>
    {
        Exp IIdentifierResultVisitor<Exp>.VisitNamespace(IdentifierResult.Namespace result)
        {
            // 에러 처리
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitGlobalFuncs(IdentifierResult.GlobalFuncs result)
        {
            // 에러 처리
            throw new NotImplementedException();
        }

        // var& x = this;
        // TODO: S& 타입 혹은 C타입을 가진 로컬 변수랑 똑같이 취급
        Exp IIdentifierResultVisitor<Exp>.VisitThis(IdentifierResult.ThisVar result)
        {
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitTypeVar(IdentifierResult.TypeVar result)
        {
            // 에러 처리
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitLambdaMemberVar(IdentifierResult.LambdaMemberVar result)
        {
            // TODO: struct member variable이랑 똑같이 처리
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitLocalVar(IdentifierResult.LocalVar result)
        {   
            if (result.Type is LocalRefType)
            {
                // 로컬 참조 타입이면, load
                // var i = 0;
                // var& x = i;
                // var& y = x; // <- 

                return new LoadExp(new LocalVarLoc(result.Name), result.Type);
            }
            else
            {
                // 아니라면 
                // var i = 0;
                // var& x = i; // <- 

                return new LocalRefExp(new LocalVarLoc(result.Name), result.Type);
            }
        }

        Exp IIdentifierResultVisitor<Exp>.VisitClass(IdentifierResult.Class result)
        {
            // 에러,
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitThisClassMemberFuncs(IdentifierResult.ThisClassMemberFuncs result)
        {
            // 에러,
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitThisClassMemberVar(IdentifierResult.ThisClassMemberVar result)
        {

            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitEnum(IdentifierResult.Enum result)
        {
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitEnumElem(IdentifierResult.EnumElem result)
        {
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitStruct(IdentifierResult.Struct result)
        {
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitThisStructMemberFuncs(IdentifierResult.ThisStructMemberFuncs result)
        {
            throw new NotImplementedException();
        }

        Exp IIdentifierResultVisitor<Exp>.VisitThisStructMemberVar(IdentifierResult.ThisStructMemberVar result)
        {
            throw new NotImplementedException();
        }
    }
}