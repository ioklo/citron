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
using static Gum.StaticAnalysis.Analyzer;
using static Gum.StaticAnalysis.Analyzer.Misc;
using static Gum.Infra.CollectionExtensions;

namespace Gum.StaticAnalysis
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class ExpAnalyzer
    {
        Analyzer analyzer;        

        public ExpAnalyzer(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        // x
        internal bool AnalyzeIdExp(
            Syntax.IdentifierExp idExp, 
            TypeValue? hintTypeValue, 
            Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var typeArgs = GetTypeValues(idExp.TypeArgs, context);

            if (!context.GetIdentifierInfo(idExp.Value, typeArgs, hintTypeValue, out var idInfo))
            {
                context.ErrorCollector.Add(idExp, $"{idExp.Value}을 찾을 수 없습니다");
                return false;
            }

            if (idInfo is IdentifierInfo.Var varIdInfo)
            {
                outTypeValue = varIdInfo.TypeValue;

                switch(varIdInfo.StorageInfo)
                {
                    case StorageInfo.ModuleGlobal mgs:          // x => global of external module
                        outExp = new IR0.ExternalGlobalVarExp(mgs.VarId);
                        break; 

                    case StorageInfo.PrivateGlobal pgs:         // x => global of this module
                        outExp = new IR0.PrivateGlobalVarExp(pgs.Name);
                        break;

                    case StorageInfo.Local ls:                  // x => local x
                        outExp = new IR0.LocalVarExp(ls.Name);
                        break;
                    
                    case StorageInfo.StaticMember sms:          // x => T.x
                        outExp = new IR0.StaticMemberVarExp();
                        break;

                    case StorageInfo.InstanceMember ims: // x => this.x
                        outExp = new IR0.InstanceMemberVarExp();
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                
                return true;
            }
            else if (idInfo is IdentifierInfo.EnumElem enumElemInfo) // S => E.S, 힌트를 사용하면 나올 수 있다, ex) E e = S; 
            {
                if (enumElemInfo.ElemInfo.FieldInfos.Length == 0)
                {
                    outTypeValue = enumElemInfo.EnumTypeValue;
                    outExp = new IR0.NewEnumExp(enumElemInfo.ElemInfo.Name); // TODO: StorageInfo.EnumElem 삭제
                    return true;
                }
                else
                {
                    // TODO: Func일때 감싸기
                    throw new NotImplementedException();
                }
            }

            // TODO: Func
            return false;
        }

        internal bool AnalyzeBoolLiteralExp(Syntax.BoolLiteralExp boolExp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = new IR0.BoolLiteralExp(boolExp.Value);
            outTypeValue = analyzer.GetBoolTypeValue();
            return true;
        }

        internal bool AnalyzeIntLiteralExp(Syntax.IntLiteralExp intExp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = new IR0.IntLiteralExp(intExp.Value);
            outTypeValue= analyzer.GetIntTypeValue();
            return true;
        }

        internal bool AnalyzeStringExp(Syntax.StringExp stringExp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            bool bResult = true;
            var strExpElems = new List<IR0.StringExpElement>();
            foreach (var elem in stringExp.Elements)
            {
                if (analyzer.AnalyzeStringExpElement(elem, context, out var strExpElem))
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
                outExp = new IR0.StringExp(strExpElems);
                outTypeValue = analyzer.GetStringTypeValue();
                return true;
            }
            else
            {
                outExp = null;
                outTypeValue = null;
                return false;
            }
        }

        internal bool IsAssignableExp(IR0.Exp exp)
        {
            switch (exp)
            {
                case IR0.ExternalGlobalVarExp _:
                case IR0.PrivateGlobalVarExp _:
                case IR0.LocalVarExp _:
                case IR0.ListIndexerExp _:
                case IR0.MemberExp _:
                    return true;

                default:
                    return false;
            }
            
        }

        // int만 지원한다
        internal bool AnalyzeIntUnaryAssignExp(
            Syntax.Exp operand,
            IR0.InternalUnaryAssignOperator op, 
            Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            var intTypeValue = analyzer.GetIntTypeValue();

            outExp = null;
            outTypeValue = null;

            if (!analyzer.AnalyzeExp(operand, null, context, out var ir0Operand, out var ir0OperandType))
                return false;

            // int type 검사
            if (!analyzer.IsAssignable(intTypeValue, ir0OperandType, context))
            {
                context.ErrorCollector.Add(operand, "++ --는 int 타입만 지원합니다");
                return false;
            }

            if (!IsAssignableExp(ir0Operand))
            {
                context.ErrorCollector.Add(operand, "++ --는 대입 가능한 식에만 적용할 수 있습니다");
                return false;
            }

            outExp = new IR0.CallInternalUnaryAssignOperator(IR0.InternalUnaryAssignOperator.PrefixDec_Int_Int, ir0Operand);
            outTypeValue = intTypeValue;
            return true;
        }

        internal bool AnalyzeUnaryOpExp(Syntax.UnaryOpExp unaryOpExp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var boolTypeValue = analyzer.GetBoolTypeValue();
            var intTypeValue = analyzer.GetIntTypeValue();
            
            if (!AnalyzeExp(unaryOpExp.Operand, null, context, out var ir0Operand, out var operandType))
                return false; // AnalyzeExp에서 에러가 생겼으므로 내부에서 에러를 추가했을 것이다. 여기서는 더 추가 하지 않는다

            switch (unaryOpExp.Kind)
            {
                case Syntax.UnaryOpKind.LogicalNot:
                    {
                        if (!analyzer.IsAssignable(boolTypeValue, operandType, context))
                        {
                            context.ErrorCollector.Add(unaryOpExp, $"{unaryOpExp.Operand}에 !를 적용할 수 없습니다. bool 타입이어야 합니다");                            
                            return false;
                        }

                        outExp = new IR0.CallInternalUnaryOperatorExp(IR0.InternalUnaryOperator.LogicalNot_Bool_Bool, new IR0.ExpInfo(ir0Operand, operandType));
                        outTypeValue = boolTypeValue;
                        return true;
                    }

                case Syntax.UnaryOpKind.Minus:
                    {
                        if (!analyzer.IsAssignable(intTypeValue, operandType, context))
                        {
                            context.ErrorCollector.Add(unaryOpExp, $"{unaryOpExp.Operand}에 -를 적용할 수 없습니다. int 타입이어야 합니다");
                            return false;
                        }

                        outExp = new IR0.CallInternalUnaryOperatorExp(IR0.InternalUnaryOperator.UnaryMinus_Int_Int, new IR0.ExpInfo(ir0Operand, operandType));
                        outTypeValue = intTypeValue;
                        return true;
                    }

                case Syntax.UnaryOpKind.PostfixInc: // e.m++ 등
                    return AnalyzeIntUnaryAssignExp(unaryOpExp, IR0.InternalUnaryAssignOperator.PostfixInc_Int_Int, context, out outExp, out outTypeValue);

                case Syntax.UnaryOpKind.PostfixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp, IR0.InternalUnaryAssignOperator.PostfixDec_Int_Int, context, out outExp, out outTypeValue);

                case Syntax.UnaryOpKind.PrefixInc:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp, IR0.InternalUnaryAssignOperator.PrefixInc_Int_Int, context, out outExp, out outTypeValue);

                case Syntax.UnaryOpKind.PrefixDec:
                    return AnalyzeIntUnaryAssignExp(unaryOpExp, IR0.InternalUnaryAssignOperator.PrefixDec_Int_Int, context, out outExp, out outTypeValue);

                default:
                    throw new InvalidOperationException();
            }
        }                

        struct InternalBinaryOperatorInfo
        {
            public TypeValue OperandType0 { get; }
            public TypeValue OperandType1 { get; }
            public TypeValue ResultType { get; }
            public IR0.InternalBinaryOperator IR0Operator { get; }

            public InternalBinaryOperatorInfo(
                TypeValue operandType0,
                TypeValue operandType1,
                TypeValue resultType,
                IR0.InternalBinaryOperator ir0Operator)            
            {
                SyntaxOperator = syntaxOperator;
                OperandType0 = operandType0;
                OperandType1 = operandType1;
                ResultType = resultType;
                IR0Operator = ir0Operator;
            }

            public static Dictionary<Syntax.BinaryOpKind, InternalBinaryOperatorInfo[]> Infos { get; }
            static InternalBinaryOperatorInfo()
            {
                var boolType = TypeValue.MakeNormal(ModuleItemId.Make("bool"));
                var intType = TypeValue.MakeNormal(ModuleItemId.Make("bool"));
                var stringType = TypeValue.MakeNormal(ModuleItemId.Make("string"));

                Infos = new Dictionary<Syntax.BinaryOpKind, InternalBinaryOperatorInfo[]>()
                {
                    { Syntax.BinaryOpKind.Multiply, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, IR0.InternalBinaryOperator.Multiply_Int_Int_Int) } },
                    { Syntax.BinaryOpKind.Divide, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, IR0.InternalBinaryOperator.Divide_Int_Int_Int) } },
                    { Syntax.BinaryOpKind.Modulo, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, IR0.InternalBinaryOperator.Modulo_Int_Int_Int) } },
                    { Syntax.BinaryOpKind.Add,  new[]{
                        new InternalBinaryOperatorInfo(intType, intType, intType, IR0.InternalBinaryOperator.Add_Int_Int_Int),
                        new InternalBinaryOperatorInfo(stringType, stringType, stringType, IR0.InternalBinaryOperator.Add_String_String_String) } },

                    { Syntax.BinaryOpKind.Subtract, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, IR0.InternalBinaryOperator.Subtract_Int_Int_Int) } },

                    { Syntax.BinaryOpKind.LessThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, IR0.InternalBinaryOperator.LessThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, IR0.InternalBinaryOperator.LessThan_String_String_Bool) } },

                    { Syntax.BinaryOpKind.GreaterThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, IR0.InternalBinaryOperator.GreaterThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, IR0.InternalBinaryOperator.GreaterThan_String_String_Bool) } },

                    { Syntax.BinaryOpKind.LessThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, IR0.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, IR0.InternalBinaryOperator.LessThanOrEqual_String_String_Bool) } },

                    { Syntax.BinaryOpKind.GreaterThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, IR0.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, IR0.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool) } },

                    { Syntax.BinaryOpKind.Equal, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, IR0.InternalBinaryOperator.Equal_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(boolType, boolType, boolType, IR0.InternalBinaryOperator.Equal_Bool_Bool_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, IR0.InternalBinaryOperator.Equal_String_String_Bool) } },
                };

            }
        }
        
        internal bool AnalyzeBinaryOpExp(
            Syntax.BinaryOpExp binaryOpExp, Context context, 
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var boolType = analyzer.GetBoolTypeValue();

            if (!AnalyzeExp(binaryOpExp.Operand0, null, context, out var operand0, out var operandType0))
                return false;

            if (!AnalyzeExp(binaryOpExp.Operand1, null, context, out var operand1, out var operandType1))
                return false;

            // 1. Assign 먼저 처리
            if (binaryOpExp.Kind == Syntax.BinaryOpKind.Assign)
            {
                if (!analyzer.IsAssignable(operandType0, operandType1, context))
                {
                    context.ErrorCollector.Add(binaryOpExp, "대입 가능하지 않습니다");
                    return false;
                }

                outExp = new IR0.AssignExp(operand0, operand1, operandType1);
                outTypeValue = operandType0;
                return true;
            }

            var infos = InternalBinaryOperatorInfo.Infos;

            // 2. NotEqual 처리
            if (binaryOpExp.Kind == Syntax.BinaryOpKind.NotEqual)
            {
                if (infos.TryGetValue(Syntax.BinaryOpKind.Equal, out var equalInfos))
                {
                    foreach (var info in equalInfos)
                    {
                        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                        if (analyzer.IsAssignable(info.OperandType0, operandType0, context) &&
                            analyzer.IsAssignable(info.OperandType1, operandType1, context))
                        {
                            var equalExp = new IR0.CallInternalBinaryOperatorExp(info.IR0Operator, new IR0.ExpInfo(operand0, operandType0), new IR0.ExpInfo(operand1, operandType1));
                            var notEqualOperand = new IR0.ExpInfo(equalExp, boolType);

                            outExp = new IR0.CallInternalUnaryOperatorExp(IR0.InternalUnaryOperator.LogicalNot_Bool_Bool, notEqualOperand);
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
                    if (analyzer.IsAssignable(info.OperandType0, operandType0, context) && 
                        analyzer.IsAssignable(info.OperandType1, operandType1, context))
                    {
                        outExp = new IR0.CallInternalBinaryOperatorExp(info.IR0Operator, new IR0.ExpInfo(operand0, operandType0), new IR0.ExpInfo(operand1, operandType1));
                        outTypeValue = info.ResultType;
                        return true;
                    }
                }
            }

            // Operator를 찾을 수 없습니다
            context.ErrorCollector.Add(binaryOpExp, $"{operandType0}와 {operandType1}를 지원하는 연산자가 없습니다");
            return false;
        }

        bool AnalyzeCallableIdentifierExp(
            Syntax.IdentifierExp callableExp, ImmutableArray<(IR0.Exp Exp, TypeValue TypeValue)> argInfos, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            // 1. this 검색

            // 2. global 검색
            var funcId = ModuleItemId.Make(callableExp.Value, callableExp.TypeArgs.Length);
            var globalFuncs = context.ModuleInfoService.GetFuncInfos(funcId).ToImmutableArray();

            if (0 < globalFuncs.Length)
            {                
                if (1 < globalFuncs.Length)
                {
                    context.ErrorCollector.Add(callableExp, $"이름이 {callableExp.Value}인 전역 함수가 여러 개 있습니다");
                    return false;
                }

                var globalFunc = globalFuncs[0];

                var typeArgs = callableExp.TypeArgs.Select(typeArg => context.GetTypeValueByTypeExp(typeArg));

                var funcValue = new FuncValue(globalFunc.FuncId, TypeArgumentList.Make(null, typeArgs));
                var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

                if (!analyzer.CheckParamTypes(callableExp, funcTypeValue.Params, argInfos.Select(info => info.TypeValue).ToList(), context))
                    return false;

                outExp = new IR0.CallFuncExp(funcValue, null, argInfos.Select(info => new IR0.ExpInfo(info.Exp, info.TypeValue)));
                outTypeValue = funcTypeValue;
                return true;
            }

            // 3. 일반 exp
            return AnalyzeCallableElseExp(callableExp, argInfos, context, out outExp, out outTypeValue);
        }

        bool AnalyzeCallableElseExp(
            Syntax.Exp callableExp, ImmutableArray<(IR0.Exp Exp, TypeValue TypeValue)> argInfos, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExp(callableExp, null, context, out var ir0Exp, out var typeValue))
                return false;

            var funcTypeValue = typeValue as TypeValue.Func;
            if (funcTypeValue == null)
            {
                context.ErrorCollector.Add(callableExp, $"호출 가능한 타입이 아닙니다");
                return false;
            }

            if (!analyzer.CheckParamTypes(callableExp, funcTypeValue.Params, argInfos.Select(info => info.TypeValue).ToList(), context))
                return false;

            // TODO: 사실 Type보다 Allocation정보가 들어가야 한다
            outExp = new IR0.CallValueExp(ir0Exp, funcTypeValue, argInfos.Select(info => new IR0.ExpInfo(info.Exp, info.TypeValue)));
            outTypeValue = funcTypeValue;
            return true;
        }
        
        // FuncValue도 같이 리턴한다
        // CallExp(F, [1]); // F(1)
        //   -> AnalyzeCallableExp(F, [Int])
        //        -> FuncValue(
        bool AnalyzeCallableExp(
            Syntax.Exp exp, 
            ImmutableArray<(IR0.Exp Exp, TypeValue TypeValue)> argInfos, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (exp is Syntax.IdentifierExp idExp)
                return AnalyzeCallableIdentifierExp(idExp, argInfos, context, out outExp, out outTypeValue);
            else
                return AnalyzeCallableElseExp(exp, argInfos, context, out outExp, out outTypeValue);
        }
        
        internal bool AnalyzeCallExp(
            Syntax.CallExp exp, TypeValue? hintTypeValue, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue) 
        {
            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료

            outExp = null;
            outTypeValue = null;

            if (!analyzer.AnalyzeExps(exp.Args, context, out var argInfos))
                return false;

            // Enum인지 확인
            if (exp.Callable is Syntax.IdentifierExp idExp)
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
                            context.ErrorCollector.Add(exp, "enum인자 개수가 맞지 않습니다");
                            return false;
                        }

                        var members = new List<IR0.NewEnumExp.Elem>();
                        foreach (var (fieldInfo, argInfo) in Zip(enumElem.ElemInfo.FieldInfos, argInfos))
                        {
                            var appliedTypeValue = context.TypeValueService.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
                            if (!analyzer.IsAssignable(appliedTypeValue, argInfo.TypeValue, context))
                            {
                                context.ErrorCollector.Add(exp, "enum의 {0}번째 인자 형식이 맞지 않습니다");
                                return false;
                            }

                            members.Add(new IR0.NewEnumExp.Elem(fieldInfo.Name, argInfo.Exp, argInfo.TypeValue));
                        }

                        outExp = new IR0.NewEnumExp(enumElem.ElemInfo.Name, members);
                        outTypeValue = enumElem.EnumTypeValue;

                        return true;
                    }
                }
            }

            // 'f'(), 'F'(), 'GetFunc()'()
            return AnalyzeCallableExp(exp.Callable, argInfos, context, out outExp, out outTypeValue);
        }
        
        internal bool AnalyzeLambdaExp(Syntax.LambdaExp exp, Context context,             
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (!analyzer.AnalyzeLambda(exp.Body, exp.Params, context, out var body, out var captureInfo, out var funcTypeValue))
            {
                outExp = null;
                outTypeValue = null;
                return false;
            }

            outExp = new IR0.LambdaExp(captureInfo, body);
            outTypeValue = funcTypeValue;
            return true;
        }

        bool AnalyzeIndexerExp(Syntax.IndexerExp exp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            if (!AnalyzeExp(exp.Object, null, context, out var obj, out var objType))
                return false;

            if (!AnalyzeExp(exp.Index, null, context, out var index, out var indexType))
                return false;

            // objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
            if (!context.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
            {
                context.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
                return false;
            }
            
            if (IsFuncStatic(funcValue.FuncId, context))
            {
                Debug.Fail("객체에 indexer가 있는데 Static입니다");
                return false;
            }

            var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

            if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }, context))
                return false;

            outExp = new IR0.ListIndexerExp(obj, objType, index, indexType);
            outTypeValue = funcTypeValue.Return;
            return true;
        }

        internal bool AnalyzeMemberCallExp(Syntax.MemberCallExp exp, Context context,
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var result = new MemberCallExpAnalyzer(this, exp, context).Analyze();
            if (result == null) return false;

            context.AddNodeInfo(exp, result.Value.NodeInfo);
            outExp = result.Value.Exp;
            outTypeValue = result.Value.TypeValue.Return;
            return true;
        }

        internal bool AnalyzeMemberExp(MemberExp memberExp, Context context, [NotNullWhen(true)] out TypeValue? outTypeValue) 
        {
            var memberExpAnalyzer = new MemberExpAnalyzer(analyzer, memberExp, context);
            var result = memberExpAnalyzer.Analyze();

            if (result != null)
            {
                context.AddNodeInfo(memberExp, result.Value.MemberExpInfo);
                outTypeValue = result.Value.TypeValue;
                return true;
            }
            else
            {
                outTypeValue = null;
                return false;
            }
        }

        internal bool AnalyzeListExp(Syntax.ListExp listExp, Context context, 
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outExp = null;
            outTypeValue = null;

            var elemExps = new List<IR0.Exp>();
            TypeValue? curElemTypeValue = null;

            foreach (var elem in listExp.Elems)
            {
                if (!AnalyzeExp(elem, null, context, out var elemExp, out var elemTypeValue))
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
                    context.ErrorCollector.Add(listExp, $"원소 {elem}의 타입이 {curElemTypeValue} 가 아닙니다");
                    return false;
                }
            }

            if (curElemTypeValue == null)
            {
                context.ErrorCollector.Add(listExp, $"리스트의 타입을 결정하지 못했습니다");
                return false;
            }

            var typeInfos = context.ModuleInfoService.GetTypeInfos(ModuleItemId.Make("List", 1)).ToImmutableArray();            

            if (typeInfos.Length != 1)
            {
                Debug.Fail("Runtime에 적합한 리스트가 없습니다");
                return false;
            }

            outExp = new IR0.ListExp(curElemTypeValue, elemExps);
            outTypeValue = TypeValue.MakeNormal(typeInfos[0].TypeId, TypeArgumentList.Make(curElemTypeValue));
            return true;
        }

        public bool AnalyzeExp(
            Syntax.Exp exp, 
            TypeValue? hintTypeValue, 
            Context context, 
            [NotNullWhen(true)] out IR0.Exp? outExp,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            switch(exp)
            {
                case Syntax.IdentifierExp idExp: return AnalyzeIdExp(idExp, hintTypeValue, context, out outExp, out outTypeValue);
                case Syntax.BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp, context, out outExp, out outTypeValue);
                case Syntax.IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp, context, out outExp, out outTypeValue);
                case Syntax.StringExp stringExp: return AnalyzeStringExp(stringExp, context, out outExp, out outTypeValue);
                case Syntax.UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp, context, out outExp, out outTypeValue);
                case Syntax.BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp, context, out outExp, out outTypeValue);
                case Syntax.CallExp callExp: return AnalyzeCallExp(callExp, hintTypeValue, context, out outExp, out outTypeValue);        
                case Syntax.LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp, context, out outExp, out outTypeValue);
                case Syntax.IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp, context, out outExp, out outTypeValue);
                case Syntax.MemberCallExp memberCallExp: return AnalyzeMemberCallExp(memberCallExp, context, out outExp, out outTypeValue);
                case Syntax.MemberExp memberExp: return AnalyzeMemberExp(memberExp, context, out outExp, out outTypeValue);
                case Syntax.ListExp listExp: return AnalyzeListExp(listExp, context, out outExp, out outTypeValue);
                default: throw new NotImplementedException();
            }
        }
    }
}
