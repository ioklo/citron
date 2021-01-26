using Gum.CompileTime;
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
using static Gum.IR0.Analyzer;
using static Gum.IR0.Analyzer.Misc;
using static Gum.Infra.CollectionExtensions;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;
using Pretune;
using System.Diagnostics.Contracts;
using Gum.Misc;

namespace Gum.IR0
{    
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {   
        [AutoConstructor]
        partial struct ExpResult
        {
            public Exp Exp { get; }
            public TypeValue TypeValue { get; }
        }

        // x
        ExpResult AnalyzeIdExp(S.IdentifierExp idExp, TypeValue? hintType)
        {
            var typeArgs = GetTypeValues(idExp.TypeArgs, context);

            var idInfo = context.GetIdentifierInfo(idExp.Value, typeArgs, hintType);
            if (idInfo == null)
                context.AddFatalError(A0501_IdExp_VariableNotFound, idExp, $"{idExp.Value}을 찾을 수 없습니다");

            switch (idInfo)
            {
                case InternalGlobalVarInfo ig:         // x => global of this module
                    return new ExpResult(new PrivateGlobalVarExp(igvi.Name), igvi.TypeValue);
                
                case LocalVarOutsideLambdaInfo lv:
                    // ??????? localOutsideLambda.Info.bNeedCapture = true; 
                    return new ExpResult(new LocalVarExp(lv.LocalVarInfo.Name), lv.LocalVarInfo.TypeValue);                    

                case LocalVarInfo lv:                  // x => local x
                    return new ExpResult(new LocalVarExp(lv.Name), lv.TypeValue);

                case StaticMemberInfo sm:          // x => T.x
                    throw new NotImplementedException();
                    // outExp = new StaticMemberExp();
                
                case InstanceMemberInfo im:        // x => this.x
                    throw new NotImplementedException();
                    //outExp = new InstanceMemberExp();

                case EnumElemInfo enumElem:         // S => E.S, 힌트를 사용하면 나올 수 있다, ex) E e = S; 
                    if (enumElem.ElemInfo.FieldInfos.Length == 0)
                    {
                        return new ExpResult(new NewEnumExp(enumElem.ElemInfo.Name, Array.Empty<NewEnumExp.Elem>()), enumElem.EnumTypeValue);
                    }
                    else
                    {
                        // TODO: Func일때 감싸기
                        throw new NotImplementedException();
                    }

                default:
                    throw new UnreachableCodeException();
            }
        }

        ExpResult AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp)
        {
            return new ExpResult(new BoolLiteralExp(boolExp.Value), TypeValues.Bool);
        }

        ExpResult AnalyzeIntLiteralExp(S.IntLiteralExp intExp)
        {
            return new ExpResult(new IntLiteralExp(intExp.Value), TypeValues.Int);
        }

        [AutoConstructor]
        partial struct StringExpResult
        {
            public StringExp Exp { get; }
            public TypeValue TypeValue { get; }
        }
        
        StringExpResult AnalyzeStringExp(S.StringExp stringExp)
        {
            var bFatal = false;

            var strExpElems = new List<StringExpElement>();
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

            return new StringExpResult(new StringExp(strExpElems), TypeValues.String);
        }

        

        // int만 지원한다
        ExpResult AnalyzeIntUnaryAssignExp(S.Exp operand, InternalUnaryAssignOperator op)
        {
            var operandResult = AnalyzeExp(operand, null);

            // int type 검사
            if (!context.IsAssignable(TypeValues.Int, operandResult.TypeValue))
                context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand, "++ --는 int 타입만 지원합니다");

            if (!context.IsAssignableExp(operandResult.Exp))
                context.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand, "++ --는 대입 가능한 식에만 적용할 수 있습니다");

            return new ExpResult(new CallInternalUnaryAssignOperator(op, operandResult.Exp), TypeValues.Int);
        }

        ExpResult AnalyzeUnaryOpExp(S.UnaryOpExp unaryOpExp)
        {
            var operandResult = AnalyzeExp(unaryOpExp.Operand, null);            

            switch (unaryOpExp.Kind)
            {
                case S.UnaryOpKind.LogicalNot:
                    {
                        if (!context.IsAssignable(TypeValues.Bool, operandResult.TypeValue))
                            context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 !를 적용할 수 없습니다. bool 타입이어야 합니다");                            

                        var operandTypeId = context.GetType(operandResult.TypeValue);

                        return new ExpResult(
                            new CallInternalUnaryOperatorExp(
                                InternalUnaryOperator.LogicalNot_Bool_Bool,
                                new ExpInfo(operandResult.Exp, operandTypeId)),
                            TypeValues.Bool);
                    }

                case S.UnaryOpKind.Minus:
                    {
                        if (!context.IsAssignable(TypeValues.Int, operandResult.TypeValue))
                            context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 -를 적용할 수 없습니다. int 타입이어야 합니다");

                        var operandTypeId = context.GetType(operandResult.TypeValue);
                        return new ExpResult(
                            new CallInternalUnaryOperatorExp(
                                InternalUnaryOperator.UnaryMinus_Int_Int,
                                new ExpInfo(operandResult.Exp, operandTypeId)),
                            TypeValues.Int);
                    }

                case S.UnaryOpKind.PostfixInc: // e.m++ 등
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PostfixInc_Int_Int);

                case S.UnaryOpKind.PostfixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PostfixDec_Int_Int);

                case S.UnaryOpKind.PrefixInc:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PrefixInc_Int_Int);

                case S.UnaryOpKind.PrefixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PrefixDec_Int_Int);

                default:
                    throw new UnreachableCodeException();
            }
        }                

        struct InternalBinaryOperatorInfo
        {
            public TypeValue OperandType0 { get; }
            public TypeValue OperandType1 { get; }
            public TypeValue ResultType { get; }
            public InternalBinaryOperator IR0Operator { get; }

            public InternalBinaryOperatorInfo(
                TypeValue operandType0,
                TypeValue operandType1,
                TypeValue resultType,
                InternalBinaryOperator ir0Operator)            
            {
                OperandType0 = operandType0;
                OperandType1 = operandType1;
                ResultType = resultType;
                IR0Operator = ir0Operator;
            }

            public static Dictionary<S.BinaryOpKind, InternalBinaryOperatorInfo[]> Infos { get; }
            static InternalBinaryOperatorInfo()
            {
                var boolType = TypeValues.Bool;
                var intType = TypeValues.Int;
                var stringType = TypeValues.String;

                Infos = new Dictionary<S.BinaryOpKind, InternalBinaryOperatorInfo[]>()
                {
                    { S.BinaryOpKind.Multiply, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, InternalBinaryOperator.Multiply_Int_Int_Int) } },
                    { S.BinaryOpKind.Divide, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, InternalBinaryOperator.Divide_Int_Int_Int) } },
                    { S.BinaryOpKind.Modulo, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, InternalBinaryOperator.Modulo_Int_Int_Int) } },
                    { S.BinaryOpKind.Add,  new[]{
                        new InternalBinaryOperatorInfo(intType, intType, intType, InternalBinaryOperator.Add_Int_Int_Int),
                        new InternalBinaryOperatorInfo(stringType, stringType, stringType, InternalBinaryOperator.Add_String_String_String) } },

                    { S.BinaryOpKind.Subtract, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, InternalBinaryOperator.Subtract_Int_Int_Int) } },

                    { S.BinaryOpKind.LessThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, InternalBinaryOperator.LessThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, InternalBinaryOperator.LessThan_String_String_Bool) } },

                    { S.BinaryOpKind.GreaterThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, InternalBinaryOperator.GreaterThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, InternalBinaryOperator.GreaterThan_String_String_Bool) } },

                    { S.BinaryOpKind.LessThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, InternalBinaryOperator.LessThanOrEqual_String_String_Bool) } },

                    { S.BinaryOpKind.GreaterThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool) } },

                    { S.BinaryOpKind.Equal, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, InternalBinaryOperator.Equal_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(boolType, boolType, boolType, InternalBinaryOperator.Equal_Bool_Bool_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, InternalBinaryOperator.Equal_String_String_Bool) } },
                };

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

                return new ExpResult(new AssignExp(operandResult0.Exp, operandResult1.Exp), operandResult0.TypeValue);
            }

            var infos = InternalBinaryOperatorInfo.Infos;

            // 2. NotEqual 처리
            if (binaryOpExp.Kind == S.BinaryOpKind.NotEqual)
            {
                if (infos.TryGetValue(S.BinaryOpKind.Equal, out var equalInfos))
                {
                    foreach (var info in equalInfos)
                    {
                        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                        if (context.IsAssignable(info.OperandType0, operandResult0.TypeValue) &&
                            context.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                        {
                            var operandTypeId0 = context.GetType(operandResult0.TypeValue);
                            var operandTypeId1 = context.GetType(operandResult1.TypeValue);

                            var equalExp = new CallInternalBinaryOperatorExp(info.IR0Operator, new ExpInfo(operandResult0.Exp, operandTypeId0), new ExpInfo(operandResult1.Exp, operandTypeId1));
                            var notEqualOperand = new ExpInfo(equalExp, Type.Bool);

                            return new ExpResult(
                                new CallInternalUnaryOperatorExp(InternalUnaryOperator.LogicalNot_Bool_Bool, notEqualOperand),
                                info.ResultType);                            
                        }
                    }
                }
            }

            // 3. InternalOperator에서 검색            
            if (infos.TryGetValue(binaryOpExp.Kind, out var matchedInfos))
            {
                foreach (var info in matchedInfos)
                {
                    // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                    if (context.IsAssignable(info.OperandType0, operandResult0.TypeValue) && 
                        context.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                    {
                        var operandTypeId0 = context.GetType(operandResult0.TypeValue);
                        var operandTypeId1 = context.GetType(operandResult1.TypeValue);

                        return new ExpResult(
                            new CallInternalBinaryOperatorExp(
                                info.IR0Operator, 
                                new ExpInfo(operandResult0.Exp, operandTypeId0), 
                                new ExpInfo(operandResult1.Exp, operandTypeId1)),
                            info.ResultType);
                    }
                }
            }

            // Operator를 찾을 수 없습니다
            context.AddFatalError(A0802_BinaryOp_OperatorNotFound, binaryOpExp, $"{operandResult0.TypeValue}와 {operandResult1.TypeValue}를 지원하는 연산자가 없습니다");
            return default; // suppress CS0161
        }

        ExpResult AnalyzeCallableIdentifierExp(S.IdentifierExp callableExp, ImmutableArray<ExpResult> argResults)
        {   
            // 1. this 검색

            // 2. global 검색            
            var globalFuncs = context.GetFuncs(NamespacePath.Root, callableExp.Value)
                .Where(func => callableExp.TypeArgs.Length <= func.TypeParams.Length) // typeParam 개수가 typeArg 개수 보다 더 많거나 같아야 한다.
                .ToImmutableArray();

            if (0 < globalFuncs.Length)
            {                
                if (1 < globalFuncs.Length)
                    context.AddFatalError(A0901_CallExp_ThereAreMultipleGlobalFunctionsHavingSameSignature, callableExp, $"이름이 {callableExp.Value}인 전역 함수가 여러 개 있습니다");

                var globalFunc = globalFuncs[0];

                var typeArgs = callableExp.TypeArgs.Select(typeArg => context.GetTypeValueByTypeExp(typeArg)).ToArray();

                var funcValue = new FuncValue(globalFunc.GetId(), typeArgs);
                var funcTypeValue = context.GetTypeValue(funcValue);

                CheckParamTypes(callableExp, funcTypeValue.Params, argResults.Select(info => info.TypeValue).ToList());

                // 내부함수라면
                var globalFuncId = globalFunc.GetId();
                if (ModuleInfoEqualityComparer.EqualsModuleName(globalFuncId.ModuleName, ModuleName.Internal))
                {
                    var funcDeclId = context.GetFuncDeclId(globalFunc.GetId());
                    if (funcDeclId != null)
                    {
                        var args = argResults.Select(info =>
                        {
                            var typeId = context.GetType(info.TypeValue);
                            return new ExpInfo(info.Exp, typeId);
                        }).ToArray();

                        return new ExpResult(
                            new CallFuncExp(
                                funcDeclId.Value,
                                typeArgs.Select(typeArg => context.GetType(typeArg)),
                                null,
                                args),
                            funcTypeValue);
                    }
                }
                else // 외부함수 처리
                {
                    throw new NotImplementedException();
                }
            }

            // 3. 일반 exp
            return AnalyzeCallableElseExp(callableExp, argResults);
        }

        ExpResult AnalyzeCallableElseExp(S.Exp callableExp, ImmutableArray<ExpResult> argResults)
        {
            var callableExpResult = AnalyzeExp(callableExp, null);

            var funcTypeValue = callableExpResult.TypeValue as TypeValue.Func;
            if (funcTypeValue == null)
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, callableExp, $"호출 가능한 타입이 아닙니다");

            CheckParamTypes(callableExp, funcTypeValue.Params, argResults.Select(info => info.TypeValue).ToList());

            var callableTypeId = context.GetType(funcTypeValue);

            var args = argResults.Select(info =>
            {
                var typeId = context.GetType(info.TypeValue);
                return new ExpInfo(info.Exp, typeId);
            }).ToArray();

            // TODO: 사실 Type보다 Allocation정보가 들어가야 한다
            return new ExpResult(
                new CallValueExp(new ExpInfo(callableExpResult.Exp, callableTypeId), args),
                funcTypeValue);
        }
        
        // FuncValue도 같이 리턴한다
        // CallExp(F, [1]); // F(1)
        //   -> AnalyzeCallableExp(F, [Int])
        //        -> FuncValue(
        ExpResult AnalyzeCallableExp(S.Exp exp, ImmutableArray<ExpResult> argResults)
        {
            if (exp is S.IdentifierExp idExp)
                return AnalyzeCallableIdentifierExp(idExp, argResults);
            else
                return AnalyzeCallableElseExp(exp, argResults);
        }
        
        ExpResult AnalyzeCallExp(S.CallExp exp, TypeValue? hintTypeValue) 
        {
            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료
            var argResults = AnalyzeExps(exp.Args);

            // Enum인지 확인
            if (exp.Callable is S.IdentifierExp idExp)
            {
                var typeArgs = GetTypeValues(idExp.TypeArgs, context);
                if (context.GetIdentifierInfo(idExp.Value, typeArgs, hintTypeValue, out var idInfo))
                {
                    if (idInfo is IdentifierInfo.EnumElem enumElem)
                    {
                        // typeArgs가 있으면 enumElem을 주지 않는다
                        Debug.Assert(idExp.TypeArgs.Length == 0);

                        // 인자 개수가 맞는지 확인
                        if (enumElem.ElemInfo.FieldInfos.Length != argResults.Length)
                            context.AddFatalError(A0903_CallExp_MismatchEnumConstructorArgCount, exp, "enum인자 개수가 맞지 않습니다");

                        var members = new List<NewEnumExp.Elem>();
                        foreach (var (fieldInfo, argResult) in Zip(enumElem.ElemInfo.FieldInfos, argResults))
                        {
                            var appliedTypeValue = context.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
                            if (!context.IsAssignable(appliedTypeValue, argResult.TypeValue))
                                context.AddFatalError(A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType, exp, "enum의 {0}번째 인자 형식이 맞지 않습니다");

                            var typeId = context.GetType(argResult.TypeValue);
                            members.Add(new NewEnumExp.Elem(fieldInfo.Name, new ExpInfo(argResult.Exp, typeId)));
                        }

                        return new ExpResult(new NewEnumExp(enumElem.ElemInfo.Name, members), enumElem.EnumTypeValue);
                    }
                }
            }

            // 'f'(), 'F'(), 'GetFunc()'()
            return AnalyzeCallableExp(exp.Callable, argResults);
        }
        
        ExpResult AnalyzeLambdaExp(S.LambdaExp exp)
        {
            var lambdaResult = AnalyzeLambda(exp, exp.Body, exp.Params);

            return new ExpResult(
                new LambdaExp(lambdaResult.CaptureInfo, exp.Params.Select(param => param.Name), lambdaResult.Body),
                lambdaResult.FuncTypeValue);
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

        ExpResult AnalyzeMemberExp(S.MemberExp memberExp)
        {
            throw new NotImplementedException();

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
            var elemExps = new List<Exp>();

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
            
            var typeId = context.GetType(curElemTypeValue);

            return new ExpResult(
                new ListExp(typeId, elemExps), 
                new TypeValue.Normal(ItemIds.List, new[] { curElemTypeValue }));
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
                case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp);
                case S.ListExp listExp: return AnalyzeListExp(listExp);
                default: throw new NotImplementedException();
            }
        }
    }
}
