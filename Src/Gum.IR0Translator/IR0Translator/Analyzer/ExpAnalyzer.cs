using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using static Gum.IR0Translator.Analyzer;
using static Gum.Infra.CollectionExtensions;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using System.Diagnostics.Contracts;
using Gum.Infra;
using Gum.Collections;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {   
        // x
        ExpResult AnalyzeIdExp(S.IdentifierExp idExp, ResolveHint resolveHint)
        {
            var result = ResolveIdentifierIdExp(idExp, resolveHint);

            switch (result)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, idExp);
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(idExp, errorResult);
                    break;

                case NotIdentifierResult:
                    throw new UnreachableCodeException();

                case LocalVarIdentifierResult localVarResult:
                    if (localVarResult.bNeedCapture)
                        context.AddLambdaCapture(new LocalLambdaCapture(localVarResult.VarName, localVarResult.TypeValue));
                    return new ExpResult(new R.LocalVarLoc(localVarResult.VarName), localVarResult.TypeValue);

                case GlobalVarIdentifierResult globalVarResult:
                    return new ExpResult(new R.GlobalVarLoc(globalVarResult.VarName), globalVarResult.TypeValue);
                
                case InstanceFuncIdentifierResult:
                    throw new NotImplementedException();

                case StaticFuncIdentifierResult:
                    throw new NotImplementedException();

                case TypeIdentifierResult:
                    context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, idExp);
                    break;
                
                case EnumElemIdentifierResult enumElemResult:
                    if (enumElemResult.IsStandalone)      // 인자 없이 있는 것
                    {
                        throw new NotImplementedException();
                        // return new ExpResult(new NewEnumExp(enumElemResult.Name, Array.Empty<NewEnumExp.Elem>()), enumElem.EnumTypeValue);
                    }
                    else
                    {
                        // TODO: Func일때 감싸기
                        throw new NotImplementedException();
                    }                    
            }

            throw new UnreachableCodeException();
        }

        ExpExpResult AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp)
        {
            return new ExpExpResult(new R.BoolLiteralExp(boolExp.Value), context.GetBoolType());
        }

        ExpExpResult AnalyzeIntLiteralExp(S.IntLiteralExp intExp)
        {
            return new ExpExpResult(new R.IntLiteralExp(intExp.Value), context.GetIntType());
        }

        [AutoConstructor]
        partial struct StringExpResult
        {
            public R.StringExp Exp { get; }
            public TypeValue TypeValue { get; }
        }
        
        ExpExpResult AnalyzeStringExp(S.StringExp stringExp)
        {
            var bFatal = false;

            var builder = ImmutableArray.CreateBuilder<R.StringExpElement>();
            foreach (var elem in stringExp.Elements)
            {
                try 
                {
                    var expElem = AnalyzeStringExpElement(elem);
                    builder.Add(expElem);
                }
                catch(FatalAnalyzeException)
                {
                    bFatal = true;
                }
            }

            if (bFatal)
                throw new FatalAnalyzeException();

            return new ExpExpResult(new R.StringExp(builder.ToImmutable()), context.GetStringType());
        }


        // int만 지원한다
        ExpExpResult AnalyzeIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
        {
            var operandResult = AnalyzeExp(operand, ResolveHint.None);

            if (operandResult is LocExpResult locResult)
            {
                // int type 검사
                if (!context.IsAssignable(context.GetIntType(), locResult.TypeValue))
                    context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);

                return new ExpExpResult(new R.CallInternalUnaryAssignOperator(op, locResult.Loc), context.GetIntType());
            }
            else
            {
                context.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
                throw new UnreachableCodeException();
            }
        }

        ExpExpResult AnalyzeUnaryOpExp(S.UnaryOpExp unaryOpExp)
        {
            var operandResult = AnalyzeExp_Exp(unaryOpExp.Operand, ResolveHint.None);

            switch (unaryOpExp.Kind)
            {
                case S.UnaryOpKind.LogicalNot:
                    {
                        if (!context.IsAssignable(context.GetBoolType(), operandResult.TypeValue))
                            context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, unaryOpExp.Operand);
                        
                        return new ExpExpResult(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                                operandResult.Exp
                            ),
                            context.GetBoolType()
                        );
                    }

                case S.UnaryOpKind.Minus:
                    {
                        if (!context.IsAssignable(context.GetIntType(), operandResult.TypeValue))
                            context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, unaryOpExp.Operand);
                        
                        return new ExpExpResult(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.UnaryMinus_Int_Int,
                                operandResult.Exp),
                            context.GetIntType());
                    }

                case S.UnaryOpKind.PostfixInc: // e.m++ 등
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PostfixInc_Int_Int);

                case S.UnaryOpKind.PostfixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PostfixDec_Int_Int);

                case S.UnaryOpKind.PrefixInc:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PrefixInc_Int_Int);

                case S.UnaryOpKind.PrefixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PrefixDec_Int_Int);

                default:
                    throw new UnreachableCodeException();
            }
        }

        ExpExpResult AnalyzeAssignBinaryOpExp(S.BinaryOpExp exp)
        {
            // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
            var destResult = AnalyzeExp(exp.Operand0, ResolveHint.None);            

            if (destResult is LocExpResult destLocResult)
            {
                var srcResult = AnalyzeExp_Exp(exp.Operand1, ResolveHint.None);

                if (!context.IsAssignable(destLocResult.TypeValue, srcResult.TypeValue))
                    context.AddFatalError(A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, exp);

                return new ExpExpResult(new R.AssignExp(destLocResult.Loc, srcResult.Exp), destLocResult.TypeValue);
            }
            else
            {
                context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                throw new UnreachableCodeException();
            }
        }
        
        ExpExpResult AnalyzeBinaryOpExp(S.BinaryOpExp binaryOpExp)
        {
            var operandResult0 = AnalyzeExp_Exp(binaryOpExp.Operand0, ResolveHint.None);
            var operandResult1 = AnalyzeExp_Exp(binaryOpExp.Operand1, ResolveHint.None);

            // 1. Assign 먼저 처리
            if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
            {
                return AnalyzeAssignBinaryOpExp(binaryOpExp);
            }            

            // 2. NotEqual 처리
            if (binaryOpExp.Kind == S.BinaryOpKind.NotEqual)
            {
                var equalInfos = internalBinOpQueryService.GetInfos(S.BinaryOpKind.Equal);                
                foreach (var info in equalInfos)
                {
                    // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                    if (context.IsAssignable(info.OperandType0, operandResult0.TypeValue) &&
                        context.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                    {   
                        var equalExp = new R.CallInternalBinaryOperatorExp(
                            info.IR0Operator,
                            operandResult0.Exp, 
                            operandResult1.Exp
                        );

                        return new ExpExpResult(
                            new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, equalExp),
                            info.ResultType);
                    }
                }
            }

            // 3. InternalOperator에서 검색            
            var matchedInfos = internalBinOpQueryService.GetInfos(binaryOpExp.Kind);
            foreach (var info in matchedInfos)
            {
                // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                if (context.IsAssignable(info.OperandType0, operandResult0.TypeValue) && 
                    context.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                {
                    return new ExpExpResult(
                        new R.CallInternalBinaryOperatorExp(
                            info.IR0Operator, 
                            operandResult0.Exp, 
                            operandResult1.Exp),
                        info.ResultType);
                }
            }

            // Operator를 찾을 수 없습니다
            context.AddFatalError(A0802_BinaryOp_OperatorNotFound, binaryOpExp);
            return default; // suppress CS0161
        }
        
        //ExpResult AnalyzeCallExpEnumElemCallable(M.EnumElemInfo enumElem, ImmutableArray<ExpResult> argResults, S.CallExp nodeForErrorReport)
        //{
        //    // 인자 개수가 맞는지 확인
        //    if (enumElem.FieldInfos.Length != argResults.Length)
        //        context.AddFatalError(A0903_CallExp_MismatchEnumConstructorArgCount, nodeForErrorReport, "enum인자 개수가 맞지 않습니다");

        //    var members = new List<NewEnumExp.Elem>();
        //    foreach (var (fieldInfo, argResult) in Zip(enumElem.FieldInfos, argResults))
        //    {
        //        var appliedTypeValue = context.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
        //        if (!context.IsAssignable(appliedTypeValue, argResult.TypeValue))
        //            context.AddFatalError(A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType, nodeForErrorReport, "enum의 {0}번째 인자 형식이 맞지 않습니다");

        //        var typeId = context.GetType(argResult.TypeValue);
        //        members.Add(new NewEnumExp.Elem(fieldInfo.Name, new ExpInfo(argResult.Exp, typeId)));
        //    }

        //    return new ExpResult(new NewEnumExp(enumElem.Name.ToString(), members), enumElem.EnumTypeValue);
        //}

        // CallExp분석에서 Callable이 Identifier인 경우 처리
        ExpResult AnalyzeCallExpFuncCallable(S.ISyntaxNode nodeForErrorReport, R.Loc? instance, FuncValue funcValue, ImmutableArray<ExpResult> argResults)
        {
            // 인자 타입 체크 
            var argTypes = ImmutableArray.CreateRange(argResults, info => info.TypeValue);            
            CheckParamTypes(nodeForErrorReport, funcValue.GetParamTypes(), argTypes);

            var args = ImmutableArray.CreateRange(argResults, argResult => argResult.WrapExp());
            var rfunc = funcValue.GetRFunc();
            var retType = funcValue.GetRetType();
            
            if (!funcValue.IsSequence)
            {
                // TODO:
                //if (!funcValue.IsInternal)
                //    throw new NotImplementedException(); // return new ExpResult(new R.ExCallFuncExp(rfunc, null, args), retType);

                return new ExpResult(new R.CallFuncExp(rfunc, instance, args), retType);
            }
            else
            {
                // TODO:
                //if (!funcValue.IsInternal)
                //  throw new NotImplementedException(); // return new ExpResult(new R.ExCallSeqFuncExp(rfunc, null, args), retType);

                return new ExpResult(new R.CallSeqFuncExp(funcValue.GetRDeclId(), funcValue.GetRTypeContext(), instance, args), retType);
                
            }
        }

        // CallExp 분석에서 Callable이 Exp인 경우 처리
        ExpExpResult AnalyzeCallExpExpCallable(R.Loc callableLoc, TypeValue callableTypeValue, ImmutableArray<ExpExpResult> argResults, S.CallExp nodeForErrorReport)
        {
            var lambdaType = callableTypeValue as LambdaTypeValue;
            if (lambdaType == null)
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport.Callable);

            var argTypes = ImmutableArray.CreateRange(argResults, info => info.TypeValue);
            CheckParamTypes(nodeForErrorReport, lambdaType.Params, argTypes);
            
            var args = argResults.Select(info => info.Exp).ToImmutableArray();

            return new ExpExpResult(
                new R.CallValueExp(callableLoc, args),
                lambdaType.Return);
        }
        
        ExpResult AnalyzeCallExp(S.CallExp exp, ResolveHint hint) 
        {
            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료


            // E e = First(2, 3); => E e = E.First(2, 3);
            // EnumHint는 어떤 모습이어야 하나 지금 힌트가 
            var callableHint = new ResolveHint(hint.TypeHint);
            var callableResult = AnalyzeExp(exp.Callable, hint);

            // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
            // 함수 이름을 먼저 찾는가
            // Argument 타입을 먼저 알아내야 하는가
            // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다
            var argResultsBuilder = ImmutableArray.CreateBuilder<ExpExpResult>();
            foreach (var arg in exp.Args)
            {
                var expResult = AnalyzeExp_Exp(arg, ResolveHint.None);
                argResultsBuilder.Add(expResult);
            }

            var argResults = argResultsBuilder.ToImmutable();
            switch (callableResult)
            {
                case NamespaceExpResult namespaceResult:
                    context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                    break;

                // callable이 타입으로 계산되면, 에러
                case TypeExpResult typeResult:
                    context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                    break;

                // F(2, 3, 4); 가 this.F인지, This.F인지 F 찾는 시점에서는 모르기 때문에, 같이 들고 와야 한다
                case FuncsExpResult funcsResult:
                    return AnalyzeCallExpFuncCallable();

                case ExpExpResult expResult:
                    var tempLoc = new R.TempLoc(expResult.Exp, expResult.TypeValue.GetRType());
                    return AnalyzeCallExpExpCallable(tempLoc, expResult.TypeValue, argResults, exp);

                case LocExpResult locResult:
                    return AnalyzeCallExpExpCallable(locResult.Loc, locResult.TypeValue, argResults, exp);

                // enum constructor, E.First
                case EnumElemExpResult enumElemResult:
                    throw new NotImplementedException();
                    // return AnalyzeCallExpEnumElemCallable(enumElemResult.Info, );
            }

            throw new UnreachableCodeException();
        }
        
        ExpResult AnalyzeLambdaExp(S.LambdaExp exp)
        {
            var lambdaResult = AnalyzeLambda(exp, exp.Body, exp.Params);

            return new ExpResult(
                new R.LambdaExp(lambdaResult.bCaptureThis, lambdaResult.CaptureLocalVars),
                lambdaResult.TypeValue);
        }

        ExpResult AnalyzeIndexerExp(S.IndexerExp exp)
        {
            throw new NotImplementedException();

            //outExp = null;
            //outTypeValue = null;

            //if (!AnalyzeExp(exp.Object, null, out var obj, out var objType))
            //    return false;

            //if (!AnalyzeExp(exp.Index, null, out var index, out var indexType))
            //    return false;

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
        
        R.Loc MakeMemberLoc(TypeValue typeValue, R.Loc instance, string name)
        {
            switch(typeValue)
            {
                case StructTypeValue structType:
                    return new R.StructMemberLoc(instance, name);
            }

            throw new NotImplementedException();
        }

        // exp.x
        ExpResult AnalyzeMemberExpExpParent(S.MemberExp memberExp, R.Loc parentLoc, TypeValue parentTypeValue, ResolveHint hint)
        {   
            var typeArgs = GetTypeValues(memberExp.MemberTypeArgs, context);
            var memberResult = parentTypeValue.GetMember(memberExp.MemberName, typeArgs, hint);

            switch(memberResult)
            {
                case MultipleCandidatesErrorItemResult:
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, memberExp);
                    break;

                case VarWithTypeArgErrorItemResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, memberExp);
                    break;

                case NotFoundItemResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                    break;

                case ValueItemResult valueResult:
                    switch(valueResult.ItemValue)
                    {
                        case TypeValue:
                            context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                            break;

                        case FuncValue:
                            throw new NotImplementedException();

                        case MemberVarValue memberVar:
                            // static인지 검사
                            if (memberVar.IsStatic)
                                context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                            var loc = MakeMemberLoc(parentTypeValue, parentLoc, memberExp.MemberName);
                            return new ExpResult(new R.LoadExp(loc), memberVar.GetTypeValue());
                    }
                    break;
            }

            throw new UnreachableCodeException();
        }

        // T.x
        ExpResult AnalyzeMemberExpTypeParent(S.MemberExp nodeForErrorReport, TypeValue parentType, string memberName, ImmutableArray<S.TypeExp> stypeArgs, ResolveHint hint)
        {
            var typeArgs = GetTypeValues(stypeArgs, context);
            var member = parentType.GetMember(memberName, typeArgs, hint);

            switch (member)
            {
                case NotFoundItemResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, nodeForErrorReport);
                    throw new UnreachableCodeException();

                case MultipleCandidatesErrorItemResult:
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                    throw new UnreachableCodeException();

                case VarWithTypeArgErrorItemResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                    throw new UnreachableCodeException();

                case ValueItemResult itemResult:
                    switch (itemResult.ItemValue)
                    {
                        // 타입이면 값으로 만들 수 없기 때문에 에러
                        case TypeValue:
                            context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, nodeForErrorReport);
                            throw new UnreachableCodeException();

                        // TODO: 함수라면 Lambda로 만들어 준다
                        case FuncValue:
                            throw new NotImplementedException();

                        // 변수라면
                        case MemberVarValue memberVarValue:

                            if (!memberVarValue.IsStatic)
                            {
                                context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                                throw new UnreachableCodeException();
                            }

                            var rparentType = parentType.GetRType();
                            var loc = new R.StaticMemberLoc(rparentType, memberName);
                            return new ExpResult(loc, memberVarValue.GetTypeValue());

                        default:
                            throw new UnreachableCodeException();
                    }

                default:
                    throw new UnreachableCodeException();
            }
        }

        void HandleErrorIdentifierResult(S.ISyntaxNode nodeForErrorReport, ErrorIdentifierResult errorResult)
        {
            switch (errorResult)
            {
                case MultipleCandiatesErrorIdentifierResult:
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                    break;
                
                case VarWithTypeArgErrorIdentifierResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                    break;

                case CantGetStaticMemberThroughInstanceIdentifierResult:
                    context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);
                    break;

                case CantGetTypeMemberThroughInstanceIdentifierResult:
                    context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, nodeForErrorReport);
                    break;

                case CantGetInstanceMemberThroughTypeIdentifierResult:
                    context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                    break;

                case FuncCantHaveMemberErrorIdentifierResult:
                    context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, nodeForErrorReport);
                    break;
            }

            throw new UnreachableCodeException();
        }
        
        // exp를 돌려주는 버전
        // parent."x"<>
        ExpResult AnalyzeMemberExp(S.MemberExp memberExp, ResolveHint hint)
        {
            var parentResult = ResolveIdentifier(memberExp.Parent, ResolveHint.None);

            switch(parentResult)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(memberExp, errorResult);
                    break;

                // Identifier가 아니라면
                case NotIdentifierResult:
                    var expParentResult = AnalyzeExpExceptId(memberExp.Parent, ResolveHint.None);
                    return AnalyzeMemberExpExpParent(memberExp, expParentResult.WrapLoc(), expParentResult.TypeValue, hint);

                case LocalVarIdentifierResult localResult:
                    if (localResult.bNeedCapture)
                        context.AddLambdaCapture(new LocalLambdaCapture(localResult.VarName, localResult.TypeValue));

                    var localVarLoc = new R.LocalVarLoc(localResult.VarName);
                    return AnalyzeMemberExpExpParent(memberExp, localVarLoc, localResult.TypeValue, hint);

                case GlobalVarIdentifierResult globalResult:
                    var globalVarLoc = new R.LocalVarLoc(globalResult.VarName);
                    return AnalyzeMemberExpExpParent(memberExp, globalVarLoc, globalResult.TypeValue, hint);

                case TypeIdentifierResult typeResult:
                    return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeValue, memberExp.MemberName, memberExp.MemberTypeArgs, hint);

                case StaticFuncIdentifierResult:
                    // 함수는 멤버변수를 가질 수 없습니다
                    context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                    break;

                case InstanceFuncIdentifierResult:
                    // 함수는 멤버변수를 가질 수 없습니다
                    context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                    break;

                case EnumElemIdentifierResult:
                    // 힌트 없이 EnumElem이 나올 수가 없다
                    break;
            }

            throw new UnreachableCodeException();

            //var memberExpAnalyzer = new MemberExpAnalyzer(analyzer, memberExp);
            //var result = memberExpAnalyzer.Analyze();

            //if (result != null)
            //{
            //    context.AddNodeInfo(memberExp, result.Value.MemberExpInfo);
            //    outTypeValue = result.Value.TypeValue;
            //    return true;
            //}
            //else
            //{
            //    outTypeValue = null;
            //    return false;
            //}
        }

        ExpResult AnalyzeListExp(S.ListExp listExp)
        {   
            var builder = ImmutableArray.CreateBuilder<R.Exp>();

            // TODO: 타입 힌트도 이용해야 할 것 같다
            TypeValue? curElemTypeValue = (listExp.ElemType != null) ? context.GetTypeValueByTypeExp(listExp.ElemType) : null;

            foreach (var elem in listExp.Elems)
            {
                var elemResult = AnalyzeExp(elem, ResolveHint.None);                    

                builder.Add(elemResult.WrapExp());

                if (curElemTypeValue == null)
                {
                    curElemTypeValue = elemResult.TypeValue;
                    continue;
                }

                if (!EqualityComparer<TypeValue>.Default.Equals(curElemTypeValue, elemResult.TypeValue))
                {
                    // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                    context.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem);
                }
            }

            if (curElemTypeValue == null)
                context.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, listExp);

            var rtype = curElemTypeValue.GetRType();

            return new ExpResult(
                new R.ListExp(rtype, builder.ToImmutable()), 
                context.GetListType(curElemTypeValue));
        }

        ExpResult AnalyzeExp(S.Exp exp, ResolveHint hint)
        {
            switch (exp)
            {
                case S.IdentifierExp idExp: return AnalyzeIdExp(idExp, hint);
                case S.BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp);
                case S.IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp);
                case S.StringExp stringExp: return AnalyzeStringExp(stringExp);
                case S.UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp);
                case S.BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp);
                case S.CallExp callExp: return AnalyzeCallExp(callExp, hint);        
                case S.LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp);
                case S.IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp);
                case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp, hint);
                case S.ListExp listExp: return AnalyzeListExp(listExp);
                default: throw new UnreachableCodeException();
            }
        }

        // 리턴값을 가능한한 Loc으로 맞춰준다
        LocExpResult AnalyzeExp_Loc(S.Exp exp, ResolveHint hint)
        {
            var result = AnalyzeExp(exp, hint);
            switch (result)
            {
                case ExpExpResult expResult:
                    return new LocExpResult(new R.TempLoc(expResult.Exp, expResult.TypeValue.GetRType()), expResult.TypeValue);

                case LocExpResult locResult:
                    return locResult;
            }

            throw new UnreachableCodeException();
        }

        ExpExpResult AnalyzeExp_Exp(S.Exp exp, ResolveHint hint)
        {
            var result = AnalyzeExp(exp, hint);
            switch(result)
            {
                case ExpExpResult expResult:
                    return expResult;

                case LocExpResult locResult:
                    return new ExpExpResult(new R.LoadExp(locResult.Loc), locResult.TypeValue);
            }

            throw new UnreachableCodeException();
        }
    }
}
