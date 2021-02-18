using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Gum.Misc;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {   
        [AutoConstructor]
        partial struct ExpResult
        {
            public R.Exp Exp { get; }
            public TypeValue TypeValue { get; }
        }        

        // x
        ExpResult AnalyzeIdExp(S.IdentifierExp idExp, TypeValue? hintType)
        {
            var result = ResolveIdentifierIdExp(idExp, hintType);

            switch (result)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, idExp, $"{idExp.Value}을 찾을 수 없습니다");
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(idExp, errorResult);
                    break;

                case ExpIdentifierResult expResult:
                    context.AddLambdaCapture(expResult.LambdaCapture);
                    return new ExpResult(expResult.Exp, expResult.TypeValue);

                case FuncIdentifierResult:
                    throw new NotImplementedException();

                case TypeIdentifierResult:
                    context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, idExp, $"타입 {idExp.Value}을 식에서 사용할 수 없습니다");
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

        ExpResult AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp)
        {
            return new ExpResult(new R.BoolLiteralExp(boolExp.Value), context.GetBoolType());
        }

        ExpResult AnalyzeIntLiteralExp(S.IntLiteralExp intExp)
        {
            return new ExpResult(new R.IntLiteralExp(intExp.Value), context.GetIntType());
        }

        [AutoConstructor]
        partial struct StringExpResult
        {
            public R.StringExp Exp { get; }
            public TypeValue TypeValue { get; }
        }
        
        StringExpResult AnalyzeStringExp(S.StringExp stringExp)
        {
            var bFatal = false;

            var strExpElems = new List<R.StringExpElement>();
            foreach (var elem in stringExp.Elements)
            {
                try 
                {
                    var expElem = AnalyzeStringExpElement(elem);
                    strExpElems.Add(expElem);
                }
                catch(FatalAnalyzeException)
                {
                    bFatal = true;
                }
            }

            if (bFatal)
                throw new FatalAnalyzeException();

            return new StringExpResult(new R.StringExp(strExpElems), context.GetStringType());
        }

        // int만 지원한다
        ExpResult AnalyzeIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
        {
            var operandResult = AnalyzeExp(operand, null);

            // int type 검사
            if (!context.IsAssignable(context.GetIntType(), operandResult.TypeValue))
                context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand, "++ --는 int 타입만 지원합니다");

            if (!context.IsAssignableExp(operandResult.Exp))
                context.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand, "++ --는 대입 가능한 식에만 적용할 수 있습니다");

            return new ExpResult(new R.CallInternalUnaryAssignOperator(op, operandResult.Exp), context.GetIntType());
        }

        ExpResult AnalyzeUnaryOpExp(S.UnaryOpExp unaryOpExp)
        {
            var operandResult = AnalyzeExp(unaryOpExp.Operand, null);            

            switch (unaryOpExp.Kind)
            {
                case S.UnaryOpKind.LogicalNot:
                    {
                        if (!context.IsAssignable(context.GetBoolType(), operandResult.TypeValue))
                            context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 !를 적용할 수 없습니다. bool 타입이어야 합니다");

                        var operandRType = operandResult.TypeValue.GetRType();

                        return new ExpResult(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                                new R.ExpInfo(operandResult.Exp, operandRType)),
                            context.GetBoolType());
                    }

                case S.UnaryOpKind.Minus:
                    {
                        if (!context.IsAssignable(context.GetIntType(), operandResult.TypeValue))
                            context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 -를 적용할 수 없습니다. int 타입이어야 합니다");

                        var operandRType = operandResult.TypeValue.GetRType();
                        return new ExpResult(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.UnaryMinus_Int_Int,
                                new R.ExpInfo(operandResult.Exp, operandRType)),
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
        
        ExpResult AnalyzeBinaryOpExp(S.BinaryOpExp binaryOpExp)
        {
            var operandResult0 = AnalyzeExp(binaryOpExp.Operand0, null);
            var operandResult1 = AnalyzeExp(binaryOpExp.Operand1, null);

            // 1. Assign 먼저 처리
            if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
            {
                if (!context.IsAssignable(operandResult0.TypeValue, operandResult1.TypeValue))
                    context.AddFatalError(A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, binaryOpExp, "대입 가능하지 않습니다");
                    
                if (!context.IsAssignableExp(operandResult0.Exp))
                    context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, binaryOpExp.Operand0, "대입 가능하지 않은 식에 대입하려고 했습니다");

                return new ExpResult(new R.AssignExp(operandResult0.Exp, operandResult1.Exp), operandResult0.TypeValue);
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
                        var operandRType0 = operandResult0.TypeValue.GetRType();
                        var operandRType1 = operandResult1.TypeValue.GetRType();

                        var equalExp = new R.CallInternalBinaryOperatorExp(info.IR0Operator, new R.ExpInfo(operandResult0.Exp, operandRType0), new R.ExpInfo(operandResult1.Exp, operandRType1));
                        var notEqualOperand = new R.ExpInfo(equalExp, R.Type.Bool);

                        return new ExpResult(
                            new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, notEqualOperand),
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
                    var operandRType0 = operandResult0.TypeValue.GetRType();
                    var operandRType1 = operandResult1.TypeValue.GetRType();

                    return new ExpResult(
                        new R.CallInternalBinaryOperatorExp(
                            info.IR0Operator, 
                            new R.ExpInfo(operandResult0.Exp, operandRType0), 
                            new R.ExpInfo(operandResult1.Exp, operandRType1)),
                        info.ResultType);
                }
            }

            // Operator를 찾을 수 없습니다
            context.AddFatalError(A0802_BinaryOp_OperatorNotFound, binaryOpExp, $"{operandResult0.TypeValue}와 {operandResult1.TypeValue}를 지원하는 연산자가 없습니다");
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
        ExpResult AnalyzeCallExpFuncCallable(S.ISyntaxNode nodeForErrorReport, FuncValue funcValue, ImmutableArray<ExpResult> argResults)
        {
            // 인자 타입 체크 
            var argTypes = ImmutableArray.CreateRange(argResults, info => info.TypeValue);            
            CheckParamTypes(nodeForErrorReport, funcValue.GetParamTypes(), argTypes);

            var args = ImmutableArray.CreateRange(argResults, argResult =>
            {
                // rType으로 
                var rtype = argResult.TypeValue.GetRType();
                return new R.ExpInfo(argResult.Exp, rtype);
            });

            var rfunc = funcValue.GetRFunc();
            var retType = funcValue.GetRetType();
            
            if (!funcValue.IsSequence)
            {
                if (funcValue.IsInternal) return new ExpResult(new R.CallFuncExp(rfunc, null, args), retType);
                else throw new NotImplementedException(); // return new ExpResult(new R.ExCallFuncExp(rfunc, null, args), retType);
            }
            else
            {
                if (funcValue.IsInternal) return new ExpResult(new R.CallSeqFuncExp(rfunc, null, args), retType);
                else throw new NotImplementedException(); // return new ExpResult(new R.ExCallSeqFuncExp(rfunc, null, args), retType);
            }
        }

        // CallExp 분석에서 Callable이 Exp인 경우 처리
        ExpResult AnalyzeCallExpExpCallable(R.Exp callable, TypeValue callableType, ImmutableArray<ExpResult> argResults, S.CallExp nodeForErrorReport)
        {
            var lambdaType = callableType as LambdaTypeValue;
            if (lambdaType == null)
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport.Callable, $"호출 가능한 타입이 아닙니다");

            var argTypes = ImmutableArray.CreateRange(argResults, info => info.TypeValue);
            CheckParamTypes(nodeForErrorReport, lambdaType.Params, argTypes);

            var callableRType = lambdaType.GetRType();
            var args = argResults.Select(info =>
            {
                var argRType = info.TypeValue.GetRType();
                return new R.ExpInfo(info.Exp, argRType);
            }).ToArray();

            // TODO: 사실 Type보다 Allocation정보가 들어가야 한다
            return new ExpResult(
                new R.CallValueExp(new R.ExpInfo(callable, callableRType), args),
                lambdaType.Return);
        }
        
        ExpResult AnalyzeCallExp(S.CallExp exp, TypeValue? hintTypeValue) 
        {
            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료
            var argResults = AnalyzeExps(exp.Args);
            var argTypes = ImmutableArray.CreateRange(argResults, result => result.TypeValue);

            // argTypes로 힌트 타입을 만들어야 한다
            var callableResult = ResolveIdentifier(exp.Callable, argTypes);

            switch(callableResult)
            {
                case NotFoundIdentifierResult _:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp, "");
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(exp, errorResult);
                    break;

                case ExpIdentifierResult expResult:
                    return AnalyzeCallExpExpCallable(expResult.Exp, expResult.TypeValue, argResults, exp);

                case FuncIdentifierResult funcResult:
                    return AnalyzeCallExpFuncCallable(exp, funcResult.FuncValue, argResults);

                case TypeIdentifierResult typeResult:
                    context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable, "");
                    break;

                case EnumElemIdentifierResult enumElemResult:
                    throw new NotImplementedException();
                    // return AnalyzeCallExpEnumElemCallable(enumElemResult.Info, );
            }

            throw new UnreachableCodeException();
        }
        
        ExpResult AnalyzeLambdaExp(S.LambdaExp exp)
        {
            var lambdaResult = AnalyzeLambda(exp, exp.Body, exp.Params);

            return new ExpResult(
                new R.LambdaExp(lambdaResult.CaptureInfo, exp.Params.Select(param => param.Name), lambdaResult.Body),
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

        ExpResult AnalyzeMemberCallExp(S.MemberCallExp exp)
        {
            throw new NotImplementedException();
            //outExp = null;
            //outTypeValue = null;

            //var result = new MemberCallExpAnalyzer(this, exp).Analyze();
            //if (result == null) return false;

            //context.AddNodeInfo(exp, result.Value.NodeInfo);
            //outExp = result.Value.Exp;
            //outTypeValue = result.Value.TypeValue.Return;
            //return true;
        }

        R.Exp MakeMemberExp(TypeValue typeValue, R.Exp instance, string name)
        {
            switch(typeValue)
            {
                case StructTypeValue structType:
                    return new R.StructMemberExp(instance, name);
            }

            throw new NotImplementedException();
        }

        // exp.x
        ExpResult AnalyzeMemberExpExpParent(S.MemberExp memberExp, R.Exp parentExp, TypeValue parentType, TypeValue? hintType)
        {   
            var typeArgs = GetTypeValues(memberExp.MemberTypeArgs, context);
            var memberResult = parentType.GetMember(memberExp.MemberName, typeArgs, hintType);

            switch(memberResult)
            {
                case MultipleCandidatesErrorItemResult:
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, memberExp, "같은 이름의 멤버가 하나이상 있습니다");
                    break;

                case VarWithTypeArgErrorItemResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");
                    break;

                case NotFoundItemResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp, $"");
                    break;

                case ValueItemResult valueResult:
                    switch(valueResult.ItemValue)
                    {
                        case TypeValue:
                            context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp, $"{memberExp.MemberName}은 타입입니다. 변수, 함수여야 합니다");
                            break;

                        case FuncValue:
                            throw new NotImplementedException();

                        case MemberVarValue memberVar:
                            // static인지 검사
                            if (memberVar.IsStatic)
                                context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp, $"인스턴스 값에서는 정적 멤버 변수를 참조할 수 없습니다");

                            var exp = MakeMemberExp(parentType, parentExp, memberExp.MemberName);
                            return new ExpResult(exp, memberVar.GetTypeValue());
                    }
                    break;
            }

            throw new UnreachableCodeException();
        }

        // T.x
        ExpResult AnalyzeMemberExpTypeParent(S.MemberExp nodeForErrorReport, TypeValue parentType, string memberName, ImmutableArray<S.TypeExp> stypeArgs, TypeValue? hintType)
        {
            var typeArgs = GetTypeValues(stypeArgs, context);
            var member = parentType.GetMember(memberName, typeArgs, hintType);

            switch (member)
            {
                case NotFoundItemResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, nodeForErrorReport, $"{memberName}은 {parentType}의 멤버가 아닙니다");
                    throw new UnreachableCodeException();

                case MultipleCandidatesErrorItemResult:
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport, "같은 이름의 멤버가 하나이상 있습니다");
                    throw new UnreachableCodeException();

                case VarWithTypeArgErrorItemResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport, "멤버변수에는 타입인자를 붙일 수 없습니다");
                    throw new UnreachableCodeException();

                case ValueItemResult itemResult:
                    switch (itemResult.ItemValue)
                    {
                        // 타입이면 값으로 만들 수 없기 때문에 에러
                        case TypeValue:
                            context.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, nodeForErrorReport, $"{memberName}은 타입입니다. 변수, 함수여야 합니다");
                            throw new UnreachableCodeException();

                        // TODO: 함수라면 Lambda로 만들어 준다
                        case FuncValue:
                            throw new NotImplementedException();

                        // 변수라면
                        case MemberVarValue memberVarValue:

                            if (!memberVarValue.IsStatic)
                            {
                                context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport, $"정적 멤버 변수만 참조할 수 있습니다");
                                throw new UnreachableCodeException();
                            }

                            var rparentType = parentType.GetRType();
                            var exp = new R.StaticMemberExp(rparentType, memberName);
                            return new ExpResult(exp, memberVarValue.GetTypeValue());

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
                    context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport, $"해당 이름에 맞는 타임, 함수, 변수를 하나로 결정할 수가 없습니다");
                    break;
                
                case VarWithTypeArgErrorIdentifierResult:
                    context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport, $"변수인데 타입인자가 들어갔습니다");
                    break;

                case CantGetStaticMemberThroughInstanceIdentifierResult:
                    context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport, $"변수인데 타입인자가 들어갔습니다");
                    break;

                case CantGetTypeMemberThroughInstanceIdentifierResult:
                    context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, nodeForErrorReport, "");
                    break;

                case CantGetInstanceMemberThroughTypeIdentifierResult:
                    context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport,"");
                    break;

                case FuncCantHaveMemberErrorIdentifierResult:
                    context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, nodeForErrorReport, "");
                    break;
            }

            throw new UnreachableCodeException();
        }

        // parent."x"<>
        ExpResult AnalyzeMemberExp(S.MemberExp memberExp, TypeValue? hintType)
        {            
            var parentResult = ResolveIdentifier(memberExp.Parent, null);

            switch(parentResult)
            {
                case NotFoundIdentifierResult:
                    context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp, $"");
                    break;

                case ErrorIdentifierResult errorResult:
                    HandleErrorIdentifierResult(memberExp, errorResult);
                    break;

                case ExpIdentifierResult expResult:
                    context.AddLambdaCapture(expResult.LambdaCapture);
                    return AnalyzeMemberExpExpParent(memberExp, expResult.Exp, expResult.TypeValue, hintType);

                case TypeIdentifierResult typeResult:
                    return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeValue, memberExp.MemberName, memberExp.MemberTypeArgs, hintType);

                case FuncIdentifierResult:
                    // 함수는 멤버변수를 가질 수 없습니다
                    context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp, "");
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
            var elemExps = new List<R.Exp>();

            // TODO: 타입 힌트도 이용해야 할 것 같다
            TypeValue? curElemTypeValue = (listExp.ElemType != null) ? context.GetTypeValueByTypeExp(listExp.ElemType) : null;

            foreach (var elem in listExp.Elems)
            {
                var elemResult = AnalyzeExp(elem, null);                    

                elemExps.Add(elemResult.Exp);

                if (curElemTypeValue == null)
                {
                    curElemTypeValue = elemResult.TypeValue;
                    continue;
                }

                if (!EqualityComparer<TypeValue>.Default.Equals(curElemTypeValue, elemResult.TypeValue))
                {
                    // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                    context.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem, $"원소 {elem}의 타입이 {curElemTypeValue} 가 아닙니다");
                }
            }

            if (curElemTypeValue == null)
                context.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, listExp, $"리스트의 타입을 결정하지 못했습니다");

            var rtype = curElemTypeValue.GetRType();

            return new ExpResult(
                new R.ListExp(rtype, elemExps), 
                context.GetListType(curElemTypeValue));
        }

        ExpResult AnalyzeExp(S.Exp exp, TypeValue? hintType)
        {
            switch(exp)
            {
                case S.IdentifierExp idExp: return AnalyzeIdExp(idExp, hintType);
                case S.BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp);
                case S.IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp);
                case S.StringExp stringExp:
                    {
                        var strExpResult = AnalyzeStringExp(stringExp);
                        return new ExpResult(strExpResult.Exp, strExpResult.TypeValue);
                    }
                case S.UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp);
                case S.BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp);
                case S.CallExp callExp: return AnalyzeCallExp(callExp, hintType);        
                case S.LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp);
                case S.IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp);
                case S.MemberCallExp memberCallExp: return AnalyzeMemberCallExp(memberCallExp);
                case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp, hintType);
                case S.ListExp listExp: return AnalyzeListExp(listExp);
                default: throw new NotImplementedException();
            }
        }
    }
}
