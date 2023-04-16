using System;
using System.Diagnostics;
using Citron.Symbol;
using Citron.Syntax;

using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Collections;

namespace Citron.Analysis
{
    struct ExpIntermediateExpTranslator : IExpVisitor<IntermediateExp>
    {
        IType? hintType;
        ScopeContext context;

        CoreExpIR0ExpTranslator coreExpTranslator;

        public static IntermediateExp Translate(Exp exp, ScopeContext context, IType? hintType)
        {
            var visitor = new ExpIntermediateExpTranslator { 
                hintType = hintType, 
                context = context,

                coreExpTranslator = new CoreExpIR0ExpTranslator(hintType, context)
            };

            return exp.Accept<ExpIntermediateExpTranslator, IntermediateExp>(ref visitor);
        }

        // x
        public IntermediateExp VisitIdentifier(IdentifierExp exp)
        {
            try
            {
                var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
                var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
                if (imExp == null)
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);

                return imExp;
            }
            catch (IdentifierResolverMultipleCandidatesException)
            {
                context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
                throw new UnreachableException();
            }
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitIdentifier(IdentifierExp exp)
            => VisitIdentifier(exp);

        // 'null'
        IntermediateExp IExpVisitor<IntermediateExp>.VisitNullLiteral(NullLiteralExp exp)
        {
            var rexp = coreExpTranslator.TranslateNullLiteral(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitBoolLiteral(BoolLiteralExp exp)
        {
            var rexp = coreExpTranslator.TranslateBoolLiteral(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitIntLiteral(IntLiteralExp exp)
        {
            var rexp = coreExpTranslator.TranslateIntLiteral(exp);
            return new IntermediateExp.IR0Exp(rexp);            
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitString(StringExp exp)
        {
            var rexp = coreExpTranslator.TranslateString(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }
        
        IntermediateExp IExpVisitor<IntermediateExp>.VisitUnaryOp(UnaryOpExp exp)
        {
            var rexp = coreExpTranslator.TranslateUnaryOp(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }
        
        IntermediateExp IExpVisitor<IntermediateExp>.VisitBinaryOp(BinaryOpExp exp)
        {
            var rexp = coreExpTranslator.TranslateBinaryOp(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitCall(CallExp exp)
        {
            var rexp = coreExpTranslator.TranslateCall(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitLambda(LambdaExp exp)
        {
            var rexp = coreExpTranslator.TranslateLambda(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitIndexer(IndexerExp exp)
        {            
            var objReExp = ExpResolvedExpTranslator.Translate(exp.Object, context, hintType: null); // type을 알기 위해서 한번 거쳤다
            var objLoc = ResolvedExpIR0LocTranslator.Translate(objReExp, context, bWrapExpAsLoc: true, exp.Object);

            var indexResult = ExpIR0ExpTranslator.Translate(exp.Index, context, hintType: null);
            var castIndexResult = BodyMisc.CastExp_Exp(indexResult, context.GetIntType(), exp.Index, context);

            // TODO: custom indexer를 만들수 있으면 좋은가
            // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

            // 리스트 타입의 경우,
            if (context.IsListType(objReExp.GetExpType(), out var itemType))
            {
                return new IntermediateExp.ListIndexer(objLoc, castIndexResult, itemType);
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
        IntermediateExp IExpVisitor<IntermediateExp>.VisitMember(MemberExp exp)
        {
            var parentImExp = ExpIntermediateExpTranslator.Translate(exp.Parent, context, hintType);

            var name = new Name.Normal(exp.MemberName);
            var typeArgs = BodyMisc.MakeTypeArgs(exp.MemberTypeArgs, context);

            return MemberParentAndIdBinder.Bind(parentImExp, name, typeArgs, context, exp);
        }

        IntermediateExp IExpVisitor<IntermediateExp>.VisitList(ListExp exp)
        {
            var rexp = coreExpTranslator.TranslateList(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        // 'new C(...)'
        IntermediateExp IExpVisitor<IntermediateExp>.VisitNew(NewExp exp)
        {
            var rexp = coreExpTranslator.TranslateNew(exp);
            return new IntermediateExp.IR0Exp(rexp);
        }

        
        //// &i를 뭐로 번역할 것인가
        //// &c.x // box ref
        //// &s.x // local ref
        //// &e.x <- 금지, 런타임에 레이아웃이 바뀔 수 있다 (추후 ref-able enum을 쓰면(레이아웃이 겹치지 않는) 되도록 허용할 수 있다)
        //IntermediateExp IExpVisitor<IntermediateExp>.VisitRef(RefExp exp)
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

        IntermediateExp IExpVisitor<IntermediateExp>.VisitBox(BoxExp exp)
        {
            throw new NotImplementedException();
        }
    }
}