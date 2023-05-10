using System;
using System.Diagnostics;
using Citron.Symbol;
using Citron.Syntax;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

using R = Citron.IR0;

namespace Citron.Analysis;

struct ExpIntermediateExpTranslator : IExpVisitor<TranslationResult<IntermediateExp>>
{
    IType? hintType;
    ScopeContext context;

    public static TranslationResult<IntermediateExp> Translate(Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpIntermediateExpTranslator { 
            hintType = hintType, 
            context = context
        };

        return exp.Accept<ExpIntermediateExpTranslator, TranslationResult<IntermediateExp>>(ref visitor);
    }

    TranslationResult<IntermediateExp> Error()
    {
        return TranslationResult.Error<IntermediateExp>();
    }

    TranslationResult<IntermediateExp> Valid(IntermediateExp exp)
    {
        return TranslationResult.Valid(exp);
    }

    TranslationResult<IntermediateExp> HandleExp(R.Exp exp)
    {
        return TranslationResult.Valid<IntermediateExp>(new IntermediateExp.IR0Exp(exp));
    }

    TranslationResult<IntermediateExp> HandleExpResult(TranslationResult<R.Exp> expResult)
    {
        if (!expResult.IsValid(out var exp))
            return TranslationResult.Error<IntermediateExp>();

        return HandleExp(exp);
    }

    // x
    public TranslationResult<IntermediateExp> VisitIdentifier(IdentifierExp exp)
    {
        try
        {
            var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
            var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (imExp == null)
            {
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
                return Error();
            }

            return Valid(imExp);
        }
        catch (IdentifierResolverMultipleCandidatesException)
        {
            context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
            return Error();
        }
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitIdentifier(IdentifierExp exp)
        => VisitIdentifier(exp);

    // 'null'
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitNullLiteral(NullLiteralExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp));        
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitBoolLiteral(BoolLiteralExp exp)
    {
        return HandleExp(new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitIntLiteral(IntLiteralExp exp)
    {
        return HandleExp(new CoreExpIR0ExpTranslator(hintType, context).TranslateIntLiteral(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitString(StringExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateString(exp));
    }
    
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitUnaryOp(UnaryOpExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOp(exp));
    }
    
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitBinaryOp(BinaryOpExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitCall(CallExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitLambda(LambdaExp exp)
    {
        return HandleExp(new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitIndexer(IndexerExp exp)
    {
        var objResult = ExpIR0LocTranslator.Translate(exp.Object, context, hintType: null, bWrapExpAsLoc: true, bDerefIfTypeIsRef: true, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!objResult.IsValid(out var objLocResult))
            return Error();

        var indexResult = ExpIR0ExpTranslator.Translate(exp.Index, context, hintType: null, bDerefIfTypeIsRef: true);
        if (!indexResult.IsValid(out var index))
            return Error();

        var castIndexResult = BodyMisc.CastExp_Exp(index, context.GetIntType(), exp.Index, context);

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        if (context.IsListType(objLocResult.LocType, out var itemType))
        {
            return Valid(new IntermediateExp.ListIndexer(objLocResult.Loc, castIndexResult, itemType));
        }
        else
        {
            throw new NotImplementedException();
        }

        //// objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
        //if (!context.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
        //{
        //    context.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
        //    return false;
        //}

        //if (IsFuncStatic(funcValue.FuncId))
        //{
        //    Debug.Fail("객체에 indexer가 있는데 Static입니다");
        //    return false;
        //}

        //var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

        //if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }))
        //    return false;

        //var listType = analyzer.GetListTypeValue()

        //// List타입인가 확인
        //if (analyzer.IsAssignable(listType, objType))
        //{
        //    var objTypeId = context.GetTypeId(objType);
        //    var indexTypeId = context.GetTypeId(indexType);

        //    outExp = new ListIndexerExp(new ExpInfo(obj, objTypeId), new ExpInfo(index, indexTypeId));
        //    outTypeValue = funcTypeValue.Return;
        //    return true;
        //}
    }
    
    // parent."x"<>
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitMember(MemberExp exp)
    {
        var parentResult = ExpIntermediateExpTranslator.Translate(exp.Parent, context, hintType);
        if (!parentResult.IsValid(out var parentImExp))
            return Error();

        var name = new Name.Normal(exp.MemberName);
        var typeArgs = BodyMisc.MakeTypeArgs(exp.MemberTypeArgs, context);

        return Valid(MemberParentAndIdBinder.Bind(parentImExp, name, typeArgs, context, exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitList(ListExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp));
    }

    // 'new C(...)'
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitNew(NewExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp));
    }

    
    //// &i를 뭐로 번역할 것인가
    //// &c.x // box ref
    //// &s.x // local ref
    //// &e.x <- 금지, 런타임에 레이아웃이 바뀔 수 있다 (추후 ref-able enum을 쓰면(레이아웃이 겹치지 않는) 되도록 허용할 수 있다)
    //TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitRef(RefExp exp)
    //{
    //    // &a.b.c.d.e, 일단 innerExp를 memberLoc으로 변경하고, 다시 순회한다
    //    //var innerResult = ExpVisitor.TranslateAsLoc(exp.InnerExp, context, hintType: null, bWrapExpAsLoc: false);
    //    //if (innerResult == null) 
    //    //    throw new NotImplementedException(); // 에러 처리

    //    //var (innerLoc, innerType) = innerResult.Value;

    //    //var refExpBuilder = new BoxRefExpBuilder();
    //    //innerLoc.Accept(ref refExpBuilder);
    //    //refExpBuilder.exp

    //    throw new NotImplementedException();
    //}

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitBox(BoxExp exp)
    {
        throw new NotImplementedException();
    }
}