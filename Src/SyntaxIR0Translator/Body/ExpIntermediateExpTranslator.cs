﻿using System;
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

    TranslationResult<IntermediateExp> HandleExp(IR0ExpResult expResult)
    {
        return TranslationResult.Valid<IntermediateExp>(new IntermediateExp.IR0Exp(expResult));
    }

    TranslationResult<IntermediateExp> HandleExpResult(TranslationResult<IR0ExpResult> result)
    {
        if (!result.IsValid(out var expResult))
            return TranslationResult.Error<IntermediateExp>();

        return HandleExp(expResult);
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
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateString_Exp(exp));
    }
    
    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitUnaryOp(UnaryOpExp exp)
    {
        // *d
        if (exp.Kind == UnaryOpKind.Deref)
        {
            var targetResult = ExpResolvedExpTranslator.Translate(exp.Operand, context, hintType: null);
            if (!targetResult.IsValid(out var target))
                return Error();

            var targetType = target.GetExpType();
            if (targetType is BoxPtrType)
                return Valid(new IntermediateExp.BoxDeref(target));

            if (targetType is LocalPtrType)
                return Valid(new IntermediateExp.LocalDeref(target));

            // 에러를 내야 할 것 같다
            throw new NotImplementedException();
        }
        else
        {
            return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOpExceptDeref(exp));
        }
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
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitIndexer(IndexerExp exp)
    {
        var objReExpResult = ExpResolvedExpTranslator.Translate(exp.Object, context, hintType: null);
        if (!objReExpResult.IsValid(out var objReExp))
            return Error();

        var indexReExpResult = ExpResolvedExpTranslator.Translate(exp.Index, context, hintType: null);
        if (!indexReExpResult.IsValid(out var indexReExp))
            return Error();

        R.Loc indexLoc;
        if (!BodyMisc.TypeEquals(indexReExp.GetExpType(), context.GetIntType()))
        {
            var indexResult = ResolvedExpIR0ExpTranslator.Translate(indexReExp, context, exp.Index);
            if (!indexResult.IsValid(out var indexExpResult))
                return Error();

            var castIndexResult = BodyMisc.CastExp_Exp(indexExpResult.Exp, indexExpResult.ExpType, context.GetIntType(), exp.Index, context);
            if (!castIndexResult.IsValid(out var castIndex))
                return Error();

            indexLoc = new R.TempLoc(castIndex, context.GetIntType());
        }
        else
        {
            var indexResult = ResolvedExpIR0LocTranslator.Translate(indexReExp, context, bWrapExpAsLoc: true, exp.Index, A2015_ResolveIdentifier_ExpressionIsNotLocation);
            if (!indexResult.IsValid(out var indexLocResult))
                return Error();

            indexLoc = indexLocResult.Loc;
        }
        

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        if (context.IsListType(objReExp.GetExpType(), out var itemType))
        {
            return Valid(new IntermediateExp.ListIndexer(objReExp, indexLoc, itemType));
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

        return IntermediateExpMemberBinder.Bind(parentImExp, name, typeArgs, context, exp);
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

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitBox(BoxExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateBox(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitIs(IsExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateIs(exp));
    }

    TranslationResult<IntermediateExp> IExpVisitor<TranslationResult<IntermediateExp>>.VisitAs(AsExp exp)
    {
        return HandleExpResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateAs(exp));
    }
}