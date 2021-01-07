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

namespace Gum.IR0
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {   
        // x
        internal bool AnalyzeIdExp(
            S.IdentifierExp idExp, 
            TypeValue? hintTypeValue, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var typeArgs = GetTypeValues(idExp.TypeArgs, context);

            if (!context.GetIdentifierInfo(idExp.Value, typeArgs, hintTypeValue, out var idInfo))
            {
                context.AddError(A0501_IdExp_VariableNotFound, idExp, $"{idExp.Value}을 찾을 수 없습니다");
                return false;
            }

            switch (idInfo)
            {
                case IdentifierInfo.ModuleGlobal mgs:          // x => global of external module
                    var varId = context.GetExternalGlobalVarId(mgs.VarValue.GetItemId()); // TypeParameter가 들어갈 일이 없으므로 AppliedId가 아니고 Id이다
                    outExp = new ExternalGlobalVarExp(varId);
                    outTypeValue = mgs.TypeValue;
                    return true;

                case IdentifierInfo.PrivateGlobal pgs:         // x => global of this module
                    outExp = new PrivateGlobalVarExp(pgs.Name);
                    outTypeValue = pgs.TypeValue;
                    return true;
                
                case IdentifierInfo.LocalOutsideLambda localOutsideLambda:
                    outExp = new LocalVarExp(localOutsideLambda.Info.LocalVarInfo.Name);
                    outTypeValue = localOutsideLambda.Info.LocalVarInfo.TypeValue;
                    localOutsideLambda.Info.bNeedCapture = true;
                    return true;

                case IdentifierInfo.Local ls:                  // x => local x
                    outExp = new LocalVarExp(ls.Name);
                    outTypeValue = ls.TypeValue;
                    return true;

                case IdentifierInfo.StaticMember sms:          // x => T.x
                    throw new NotImplementedException();
                    // outExp = new StaticMemberExp();
                
                case IdentifierInfo.InstanceMember ims:        // x => this.x
                    throw new NotImplementedException();
                //outExp = new InstanceMemberExp();

                case IdentifierInfo.EnumElem enumElem:         // S => E.S, 힌트를 사용하면 나올 수 있다, ex) E e = S; 
                    if (enumElem.ElemInfo.FieldInfos.Length == 0)
                    {
                        outTypeValue = enumElem.EnumTypeValue;
                        outExp = new NewEnumExp(enumElem.ElemInfo.Name, Array.Empty<NewEnumExp.Elem>());
                        return true;
                    }
                    else
                    {
                        // TODO: Func일때 감싸기
                        throw new NotImplementedException();
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        internal bool AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = new BoolLiteralExp(boolExp.Value);
            outTypeValue = TypeValues.Bool;
            return true;
        }

        internal bool AnalyzeIntLiteralExp(S.IntLiteralExp intExp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = new IntLiteralExp(intExp.Value);
            outTypeValue = TypeValues.Int;
            return true;
        }

        internal bool AnalyzeStringExp(S.StringExp stringExp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            bool bResult = true;
            var strExpElems = new List<StringExpElement>();
            foreach (var elem in stringExp.Elements)
            {
                if (AnalyzeStringExpElement(elem, out var strExpElem))
                {
                    strExpElems.Add(strExpElem);
                }
                else
                {
                    bResult = false;
                }
            }

            if (bResult)
            {
                outExp = new StringExp(strExpElems);
                outTypeValue = TypeValues.String;
                return true;
            }
            else
            {
                outExp = null;
                outTypeValue = null;
                return false;
            }
        }

        bool IsAssignableExp(Exp exp)
        {
            switch (exp)
            {
                case LocalVarExp localVarExp:

                    // 람다 바깥에 있다면 대입 불가능하다
                    if (context.IsLocalVarOutsideLambda(localVarExp.Name))
                        return false;

                    return true;

                case ExternalGlobalVarExp _:
                case PrivateGlobalVarExp _:                    
                case ListIndexerExp _:
                case StaticMemberExp _:
                case StructMemberExp _:
                case ClassMemberExp _:
                case EnumMemberExp _:
                    return true;

                default:
                    return false;
            }
            
        }

        // int만 지원한다
        internal bool AnalyzeIntUnaryAssignExp(
            S.Exp operand,
            InternalUnaryAssignOperator op, 
            
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExp(operand, null, out var ir0Operand, out var ir0OperandType))
                return false;

            // int type 검사
            if (!IsAssignable(TypeValues.Int, ir0OperandType))
            {
                context.AddError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand, "++ --는 int 타입만 지원합니다");
                return false;
            }

            if (!IsAssignableExp(ir0Operand))
            {
                context.AddError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand, "++ --는 대입 가능한 식에만 적용할 수 있습니다");
                return false;
            }

            outExp = new CallInternalUnaryAssignOperator(op, ir0Operand);
            outTypeValue = TypeValues.Int;
            return true;
        }

        internal bool AnalyzeUnaryOpExp(S.UnaryOpExp unaryOpExp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;            
            
            if (!AnalyzeExp(unaryOpExp.Operand, null, out var ir0Operand, out var operandType))
                return false; // AnalyzeExp에서 에러가 생겼으므로 내부에서 에러를 추가했을 것이다. 여기서는 더 추가 하지 않는다

            switch (unaryOpExp.Kind)
            {
                case S.UnaryOpKind.LogicalNot:
                    {
                        if (!IsAssignable(TypeValues.Bool, operandType))
                        {
                            context.AddError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 !를 적용할 수 없습니다. bool 타입이어야 합니다");                            
                            return false;
                        }

                        var operandTypeId = context.GetType(operandType);
                        outExp = new CallInternalUnaryOperatorExp(InternalUnaryOperator.LogicalNot_Bool_Bool, new ExpInfo(ir0Operand, operandTypeId));
                        outTypeValue = TypeValues.Bool;
                        return true;
                    }

                case S.UnaryOpKind.Minus:
                    {
                        if (!IsAssignable(TypeValues.Int, operandType))
                        {
                            context.AddError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, unaryOpExp.Operand, $"{unaryOpExp.Operand}에 -를 적용할 수 없습니다. int 타입이어야 합니다");
                            return false;
                        }

                        var operandTypeId = context.GetType(operandType);
                        outExp = new CallInternalUnaryOperatorExp(InternalUnaryOperator.UnaryMinus_Int_Int, new ExpInfo(ir0Operand, operandTypeId));
                        outTypeValue = TypeValues.Int;
                        return true;
                    }

                case S.UnaryOpKind.PostfixInc: // e.m++ 등
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PostfixInc_Int_Int, out outExp, out outTypeValue);

                case S.UnaryOpKind.PostfixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PostfixDec_Int_Int, out outExp, out outTypeValue);

                case S.UnaryOpKind.PrefixInc:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PrefixInc_Int_Int, out outExp, out outTypeValue);

                case S.UnaryOpKind.PrefixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, InternalUnaryAssignOperator.PrefixDec_Int_Int, out outExp, out outTypeValue);

                default:
                    throw new InvalidOperationException();
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
        
        internal bool AnalyzeBinaryOpExp(
            S.BinaryOpExp binaryOpExp,  
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExp(binaryOpExp.Operand0, null, out var operand0, out var operandType0))
                return false;

            if (!AnalyzeExp(binaryOpExp.Operand1, null, out var operand1, out var operandType1))
                return false;

            // 1. Assign 먼저 처리
            if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
            {
                if (!IsAssignable(operandType0, operandType1))
                {
                    context.AddError(A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, binaryOpExp, "대입 가능하지 않습니다");
                    return false;
                }
                
                if (!IsAssignableExp(operand0))
                {
                    context.AddError(A0803_BinaryOp_LeftOperandIsNotAssignable, binaryOpExp.Operand0, "대입 가능하지 않은 식에 대입하려고 했습니다");
                    return false;
                }

                outExp = new AssignExp(operand0, operand1);
                outTypeValue = operandType0;
                return true;
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
                        if (IsAssignable(info.OperandType0, operandType0) &&
                            IsAssignable(info.OperandType1, operandType1))
                        {
                            var operandTypeId0 = context.GetType(operandType0);
                            var operandTypeId1 = context.GetType(operandType1);

                            var equalExp = new CallInternalBinaryOperatorExp(info.IR0Operator, new ExpInfo(operand0, operandTypeId0), new ExpInfo(operand1, operandTypeId1));
                            var notEqualOperand = new ExpInfo(equalExp, Type.Bool);

                            outExp = new CallInternalUnaryOperatorExp(InternalUnaryOperator.LogicalNot_Bool_Bool, notEqualOperand);
                            outTypeValue = info.ResultType;
                            return true;
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
                    if (IsAssignable(info.OperandType0, operandType0) && 
                        IsAssignable(info.OperandType1, operandType1))
                    {
                        var operandTypeId0 = context.GetType(operandType0);
                        var operandTypeId1 = context.GetType(operandType1);

                        outExp = new CallInternalBinaryOperatorExp(info.IR0Operator, new ExpInfo(operand0, operandTypeId0), new ExpInfo(operand1, operandTypeId1));
                        outTypeValue = info.ResultType;
                        return true;
                    }
                }
            }

            // Operator를 찾을 수 없습니다
            context.AddError(A0802_BinaryOp_OperatorNotFound, binaryOpExp, $"{operandType0}와 {operandType1}를 지원하는 연산자가 없습니다");
            return false;
        }

        bool AnalyzeCallableIdentifierExp(
            S.IdentifierExp callableExp, ImmutableArray<(Exp Exp, TypeValue TypeValue)> argInfos, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            // 1. this 검색

            // 2. global 검색            
            var globalFuncs = context.GetFuncs(NamespacePath.Root, callableExp.Value)
                .Where(func => callableExp.TypeArgs.Length <= func.TypeParams.Length) // typeParam 개수가 typeArg 개수 보다 더 많거나 같아야 한다.
                .ToImmutableArray();

            if (0 < globalFuncs.Length)
            {                
                if (1 < globalFuncs.Length)
                {
                    context.AddError(A0901_CallExp_ThereAreMultipleGlobalFunctionsHavingSameSignature, callableExp, $"이름이 {callableExp.Value}인 전역 함수가 여러 개 있습니다");
                    return false;
                }

                var globalFunc = globalFuncs[0];

                var typeArgs = callableExp.TypeArgs.Select(typeArg => context.GetTypeValueByTypeExp(typeArg)).ToArray();

                var funcValue = new FuncValue(globalFunc.GetId(), typeArgs);
                var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

                if (!CheckParamTypes(callableExp, funcTypeValue.Params, argInfos.Select(info => info.TypeValue).ToList()))
                    return false;

                // 내부함수라면
                var funcDeclId = context.GetFuncDeclId(globalFunc.GetId());
                if (funcDeclId != null)
                {
                    var args = argInfos.Select(info =>
                    {
                        var typeId = context.GetType(info.TypeValue);
                        return new ExpInfo(info.Exp, typeId);
                    }).ToArray();

                    outExp = new CallFuncExp(
                        funcDeclId.Value,
                        typeArgs.Select(typeArg => context.GetType(typeArg)),
                        null,
                        args);

                    outTypeValue = funcTypeValue;
                    return true;
                }
                else // 외부함수 처리
                {   
                    throw new NotImplementedException();
                }
            }

            // 3. 일반 exp
            return AnalyzeCallableElseExp(callableExp, argInfos, out outExp, out outTypeValue);
        }

        bool AnalyzeCallableElseExp(
            S.Exp callableExp, ImmutableArray<(Exp Exp, TypeValue TypeValue)> argInfos, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExp(callableExp, null, out var ir0Exp, out var typeValue))
                return false;

            var funcTypeValue = typeValue as TypeValue.Func;
            if (funcTypeValue == null)
            {
                context.AddError(A0902_CallExp_CallableExpressionIsNotCallable, callableExp, $"호출 가능한 타입이 아닙니다");
                return false;
            }

            if (!CheckParamTypes(callableExp, funcTypeValue.Params, argInfos.Select(info => info.TypeValue).ToList()))
                return false;

            var callableTypeId = context.GetType(funcTypeValue);

            var args = argInfos.Select(info =>
            {
                var typeId = context.GetType(info.TypeValue);
                return new ExpInfo(info.Exp, typeId);
            }).ToArray();

            // TODO: 사실 Type보다 Allocation정보가 들어가야 한다
            outExp = new CallValueExp(new ExpInfo(ir0Exp, callableTypeId), args);
            outTypeValue = funcTypeValue;
            return true;
        }
        
        // FuncValue도 같이 리턴한다
        // CallExp(F, [1]); // F(1)
        //   -> AnalyzeCallableExp(F, [Int])
        //        -> FuncValue(
        bool AnalyzeCallableExp(
            S.Exp exp, 
            ImmutableArray<(Exp Exp, TypeValue TypeValue)> argInfos, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (exp is S.IdentifierExp idExp)
                return AnalyzeCallableIdentifierExp(idExp, argInfos, out outExp, out outTypeValue);
            else
                return AnalyzeCallableElseExp(exp, argInfos, out outExp, out outTypeValue);
        }
        
        internal bool AnalyzeCallExp(
            S.CallExp exp, TypeValue? hintTypeValue, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue) 
        {
            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료

            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExps(exp.Args, out var argInfos))
                return false;

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
                        if (enumElem.ElemInfo.FieldInfos.Length != argInfos.Length)
                        {
                            context.AddError(A0903_CallExp_MismatchEnumConstructorArgCount, exp, "enum인자 개수가 맞지 않습니다");
                            return false;
                        }

                        var members = new List<NewEnumExp.Elem>();
                        foreach (var (fieldInfo, argInfo) in Zip(enumElem.ElemInfo.FieldInfos, argInfos))
                        {
                            var appliedTypeValue = context.TypeValueService.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
                            if (!IsAssignable(appliedTypeValue, argInfo.TypeValue))
                            {
                                context.AddError(A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType, exp, "enum의 {0}번째 인자 형식이 맞지 않습니다");
                                return false;
                            }

                            var typeId = context.GetType(argInfo.TypeValue);
                            members.Add(new NewEnumExp.Elem(fieldInfo.Name, new ExpInfo(argInfo.Exp, typeId)));
                        }

                        outExp = new NewEnumExp(enumElem.ElemInfo.Name, members);
                        outTypeValue = enumElem.EnumTypeValue;

                        return true;
                    }
                }
            }

            // 'f'(), 'F'(), 'GetFunc()'()
            return AnalyzeCallableExp(exp.Callable, argInfos, out outExp, out outTypeValue);
        }
        
        internal bool AnalyzeLambdaExp(S.LambdaExp exp,              
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (!AnalyzeLambda(exp, exp.Body, exp.Params, out var body, out var captureInfo, out var funcTypeValue))
            {
                outExp = null;
                outTypeValue = null;
                return false;
            }

            outExp = new LambdaExp(captureInfo, exp.Params.Select(param => param.Name), body);
            outTypeValue = funcTypeValue;
            return true;
        }

        bool AnalyzeIndexerExp(S.IndexerExp exp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
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

        internal bool AnalyzeMemberCallExp(S.MemberCallExp exp, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
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

        internal bool AnalyzeMemberExp(S.MemberExp memberExp,  [NotNullWhen(true)] out IR0.Exp? outExp, [NotNullWhen(true)] out TypeValue? outTypeValue) 
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

        internal bool AnalyzeListExp(S.ListExp listExp,  
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var elemExps = new List<Exp>();

            // TODO: 타입 힌트도 이용해야 할 것 같다
            TypeValue? curElemTypeValue = (listExp.ElemType != null) ? context.GetTypeValueByTypeExp(listExp.ElemType) : null;

            foreach (var elem in listExp.Elems)
            {
                if (!AnalyzeExp(elem, null, out var elemExp, out var elemTypeValue))
                    return false;

                elemExps.Add(elemExp);

                if (curElemTypeValue == null)
                {
                    curElemTypeValue = elemTypeValue;
                    continue;
                }

                if (!EqualityComparer<TypeValue>.Default.Equals(curElemTypeValue, elemTypeValue))
                {
                    // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                    context.AddError(A1702_ListExp_MismatchBetweenElementTypes, elem, $"원소 {elem}의 타입이 {curElemTypeValue} 가 아닙니다");
                    return false;
                }
            }

            if (curElemTypeValue == null)
            {
                context.AddError(A1701_ListExp_CantInferElementTypeWithEmptyElement, listExp, $"리스트의 타입을 결정하지 못했습니다");
                return false;
            }
            
            var typeId = context.GetType(curElemTypeValue);
            outExp = new ListExp(typeId, elemExps);
            outTypeValue = new TypeValue.Normal(ItemIds.List, new[] { curElemTypeValue });
            return true;
        }

        public bool AnalyzeExp(
            S.Exp exp, 
            TypeValue? hintTypeValue, 
            [NotNullWhen(true)] out Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            switch(exp)
            {
                case S.IdentifierExp idExp: return AnalyzeIdExp(idExp, hintTypeValue, out outExp, out outTypeValue);
                case S.BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp, out outExp, out outTypeValue);
                case S.IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp, out outExp, out outTypeValue);
                case S.StringExp stringExp: return AnalyzeStringExp(stringExp, out outExp, out outTypeValue);
                case S.UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp, out outExp, out outTypeValue);
                case S.BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp, out outExp, out outTypeValue);
                case S.CallExp callExp: return AnalyzeCallExp(callExp, hintTypeValue, out outExp, out outTypeValue);        
                case S.LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp, out outExp, out outTypeValue);
                case S.IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp, out outExp, out outTypeValue);
                case S.MemberCallExp memberCallExp: return AnalyzeMemberCallExp(memberCallExp, out outExp, out outTypeValue);
                case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp, out outExp, out outTypeValue);
                case S.ListExp listExp: return AnalyzeListExp(listExp, out outExp, out outTypeValue);
                default: throw new NotImplementedException();
            }
        }
    }
}
