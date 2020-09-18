using Gum.CompileTime;
using Gum.Syntax;
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
        internal bool AnalyzeIdExp(IdentifierExp idExp, TypeValue? hintTypeValue, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
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
                context.AddNodeInfo(idExp, new IdentifierExpInfo(varIdInfo.StorageInfo));
                return true;
            }
            else if (idInfo is IdentifierInfo.EnumElem enumElemInfo)
            {
                if (enumElemInfo.ElemInfo.FieldInfos.Length == 0)
                {
                    outTypeValue = enumElemInfo.EnumTypeValue;
                    // TODO: IdentifierExpInfo를 EnumElem에 맞게 분기시켜야 할 것 같다, EnumElem이 StorageInfo는 아니다
                    context.AddNodeInfo(idExp, new IdentifierExpInfo(StorageInfo.MakeEnumElem(enumElemInfo.ElemInfo.Name)));
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

        internal bool AnalyzeBoolLiteralExp(BoolLiteralExp boolExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {   
            typeValue = analyzer.GetBoolTypeValue();
            return true;
        }

        internal bool AnalyzeIntLiteralExp(IntLiteralExp intExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            typeValue = analyzer.GetIntTypeValue();
            return true;
        }

        internal bool AnalyzeStringExp(StringExp stringExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            foreach(var elem in stringExp.Elements)
                analyzer.AnalyzeStringExpElement(elem, context);

            typeValue = analyzer.GetStringTypeValue();
            return true;
        }

        internal bool AnalyzeUnaryOpExp(UnaryOpExp unaryOpExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            typeValue = null;

            var boolTypeValue = analyzer.GetBoolTypeValue();
            var intTypeValue = analyzer.GetIntTypeValue();
            
            if (!AnalyzeExp(unaryOpExp.Operand, null, context, out var operandTypeValue))            
                return false; // AnalyzeExp에서 에러가 생겼으므로 내부에서 에러를 추가했을 것이다. 여기서는 더 추가 하지 않는다

            switch (unaryOpExp.Kind)
            {
                case UnaryOpKind.LogicalNot:
                    {
                        if (!analyzer.IsAssignable(boolTypeValue, operandTypeValue, context))
                        {
                            context.ErrorCollector.Add(unaryOpExp, $"{unaryOpExp.Operand}에 !를 적용할 수 없습니다. bool 타입이어야 합니다");                            
                            return false;
                        }

                        typeValue = boolTypeValue;
                        return true;
                    }
                
                case UnaryOpKind.PostfixInc: // e.m++ 등
                case UnaryOpKind.PostfixDec:
                case UnaryOpKind.PrefixInc:
                case UnaryOpKind.PrefixDec:
                    return AnalyzeUnaryAssignExp(unaryOpExp, context, out typeValue);

                case UnaryOpKind.Minus:
                    {
                        if (!analyzer.IsAssignable(intTypeValue, operandTypeValue, context))
                        {
                            context.ErrorCollector.Add(unaryOpExp, $"{unaryOpExp.Operand}에 -를 적용할 수 없습니다. int 타입이어야 합니다");
                            return false;
                        }

                        typeValue = intTypeValue;
                        return true;
                    }

                default:
                    context.ErrorCollector.Add(unaryOpExp, $"{operandTypeValue}를 지원하는 연산자가 없습니다");
                    return false;
            }
        }        

        

        internal bool AnalyzeBinaryOpExp(BinaryOpExp binaryOpExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {   
            if (binaryOpExp.Kind == BinaryOpKind.Assign)
                return AnalyzeBinaryAssignExp(binaryOpExp, context, out typeValue);

            typeValue = null;

            var boolTypeValue = analyzer.GetBoolTypeValue();
            var intTypeValue = analyzer.GetIntTypeValue();
            var stringTypeValue = analyzer.GetStringTypeValue();

            if (!AnalyzeExp(binaryOpExp.Operand0, null, context, out var operandTypeValue0))
                return false;

            if (!AnalyzeExp(binaryOpExp.Operand1, null, context, out var operandTypeValue1))
                return false;

            if (binaryOpExp.Kind == BinaryOpKind.Equal || binaryOpExp.Kind == BinaryOpKind.NotEqual)
            {   
                if (!EqualityComparer<TypeValue>.Default.Equals(operandTypeValue0, operandTypeValue1))
                {
                    context.ErrorCollector.Add(binaryOpExp, $"{operandTypeValue0}와 {operandTypeValue1}을 비교할 수 없습니다");
                    return false;
                }

                if (analyzer.IsAssignable(boolTypeValue, operandTypeValue0, context) &&
                    analyzer.IsAssignable(boolTypeValue, operandTypeValue1, context))
                {
                    context.AddNodeInfo(binaryOpExp, new BinaryOpExpInfo(BinaryOpExpInfo.OpType.Bool));
                }
                else if (analyzer.IsAssignable(intTypeValue, operandTypeValue0, context) &&
                    analyzer.IsAssignable(intTypeValue, operandTypeValue1, context))
                {
                    context.AddNodeInfo(binaryOpExp, new BinaryOpExpInfo(BinaryOpExpInfo.OpType.Integer));
                }
                else if (analyzer.IsAssignable(stringTypeValue, operandTypeValue0, context) &&
                    analyzer.IsAssignable(stringTypeValue, operandTypeValue1, context))
                {
                    context.AddNodeInfo(binaryOpExp, new BinaryOpExpInfo(BinaryOpExpInfo.OpType.String));
                }
                else
                {
                    context.ErrorCollector.Add(binaryOpExp, $"bool, int, string만 비교를 지원합니다");
                    return false;
                }

                typeValue = boolTypeValue;
                return true;
            }

            // TODO: 일단 하드코딩, Evaluator랑 지원하는 것들이 똑같아야 한다
            if (analyzer.IsAssignable(boolTypeValue, operandTypeValue0, context))
            {
                if (!analyzer.IsAssignable(boolTypeValue, operandTypeValue1, context))
                {
                    context.ErrorCollector.Add(binaryOpExp, $"{operandTypeValue1}은 bool 형식이어야 합니다");
                    return false;
                }

                switch (binaryOpExp.Kind)
                {
                    default:
                        context.ErrorCollector.Add(binaryOpExp, $"bool 형식에 적용할 수 있는 연산자가 아닙니다");
                        return false;
                }
            }
            else if (analyzer.IsAssignable(intTypeValue, operandTypeValue0, context))
            {
                if (!analyzer.IsAssignable(intTypeValue, operandTypeValue1, context))
                {
                    context.ErrorCollector.Add(binaryOpExp, $"{operandTypeValue1}은 int 형식이어야 합니다");
                    return false;
                }

                // 하드코딩
                context.AddNodeInfo(binaryOpExp, new BinaryOpExpInfo(BinaryOpExpInfo.OpType.Integer));

                switch (binaryOpExp.Kind)
                {
                    case BinaryOpKind.Multiply:
                    case BinaryOpKind.Divide:
                    case BinaryOpKind.Modulo:
                    case BinaryOpKind.Add:
                    case BinaryOpKind.Subtract:
                        typeValue = intTypeValue;
                        return true;

                    case BinaryOpKind.LessThan:
                    case BinaryOpKind.GreaterThan:
                    case BinaryOpKind.LessThanOrEqual:
                    case BinaryOpKind.GreaterThanOrEqual:
                        typeValue = boolTypeValue;
                        return true;

                    default:
                        context.ErrorCollector.Add(binaryOpExp, $"int 형식에 적용할 수 있는 연산자가 아닙니다");
                        return false;
                }
            }
            else if (analyzer.IsAssignable(stringTypeValue, operandTypeValue0, context))
            {
                if (!analyzer.IsAssignable(stringTypeValue, operandTypeValue1, context))
                {
                    context.ErrorCollector.Add(binaryOpExp, $"{operandTypeValue1}은 string 형식이어야 합니다");
                    return false;
                }

                // 하드코딩
                context.AddNodeInfo(binaryOpExp, new BinaryOpExpInfo(BinaryOpExpInfo.OpType.String));

                switch (binaryOpExp.Kind)
                {
                    case BinaryOpKind.Add:
                        typeValue = stringTypeValue;
                        return true;

                    case BinaryOpKind.LessThan:
                    case BinaryOpKind.GreaterThan:
                    case BinaryOpKind.LessThanOrEqual:
                    case BinaryOpKind.GreaterThanOrEqual:
                        typeValue = boolTypeValue;
                        return true;

                    default:
                        context.ErrorCollector.Add(binaryOpExp, $"string 형식에 적용할 수 있는 연산자가 아닙니다");
                        return false;
                }
            }

            context.ErrorCollector.Add(binaryOpExp, $"{operandTypeValue0}와 {operandTypeValue1}를 지원하는 연산자가 없습니다");
            return false;
        }

        bool AnalyzeCallableIdentifierExp(
            IdentifierExp exp, ImmutableArray<TypeValue> args, Context context,
            [NotNullWhen(returnValue: true)] out (FuncValue? FuncValue, TypeValue.Func TypeValue)? outValue)
        {
            // 1. this 검색

            // 2. global 검색
            var funcId = ModuleItemId.Make(exp.Value, exp.TypeArgs.Length);
            var globalFuncs = context.ModuleInfoService.GetFuncInfos(funcId).ToImmutableArray();

            if (0 < globalFuncs.Length)
            {                
                if (1 < globalFuncs.Length)
                {
                    context.ErrorCollector.Add(exp, $"이름이 {exp.Value}인 전역 함수가 여러 개 있습니다");
                    outValue = null;
                    return false;                    
                }

                var globalFunc = globalFuncs[0];

                var typeArgs = exp.TypeArgs.Select(typeArg => context.GetTypeValueByTypeExp(typeArg));

                var funcValue = new FuncValue(globalFunc.FuncId, TypeArgumentList.Make(null, typeArgs));
                var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

                if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, args, context))
                {
                    outValue = null;
                    return false;
                }
                
                outValue = (funcValue, funcTypeValue);
                return true;
            }

            // 3. 일반 exp
            return AnalyzeCallableElseExp(exp, args, context, out outValue);
        }


        bool AnalyzeCallableElseExp(
            Exp exp, ImmutableArray<TypeValue> args, Context context,
            [NotNullWhen(returnValue: true)] out (FuncValue? FuncValue, TypeValue.Func TypeValue)? outValue)
        {
            if (!AnalyzeExp(exp, null, context, out var typeValue))
            {
                outValue = null;
                return false;
            }

            var funcTypeValue = typeValue as TypeValue.Func;
            if (funcTypeValue == null)
            {
                context.ErrorCollector.Add(exp, $"호출 가능한 타입이 아닙니다");
                outValue = null;
                return false;
            }

            if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, args, context))
            {
                outValue = null;
                return false;
            }

            outValue = (null, funcTypeValue);
            return true;
        }
        
        // FuncValue도 같이 리턴한다
        // CallExp(F, [1]); // F(1)
        //   -> AnalyzeCallableExp(F, [Int])
        //        -> FuncValue(
        bool AnalyzeCallableExp(
            Exp exp, 
            ImmutableArray<TypeValue> args, Context context, 
            [NotNullWhen(returnValue: true)] out (FuncValue? FuncValue, TypeValue.Func TypeValue)? outValue)
        {
            if (exp is IdentifierExp idExp)
                return AnalyzeCallableIdentifierExp(idExp, args, context, out outValue);
            else
                return AnalyzeCallableElseExp(exp, args, context, out outValue);
        }
        
        internal bool AnalyzeCallExp(CallExp exp, TypeValue? hintTypeValue, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue) 
        {
            (SyntaxNodeInfo NodeInfo, TypeValue TypeValue)? AnalyzeEnumValueOrNormal(ImmutableArray<TypeValue> args)
            {
                if (exp.Callable is IdentifierExp idExp)
                {
                    var typeArgs = GetTypeValues(idExp.TypeArgs, context);                    
                    if (context.GetIdentifierInfo(idExp.Value, typeArgs, hintTypeValue, out var idInfo))
                    {
                        if (idInfo is IdentifierInfo.EnumElem enumElem)
                        {
                            // typeArgs가 있으면 enumElem을 주지 않는다
                            Debug.Assert(idExp.TypeArgs.Length == 0);

                            // 인자 개수가 맞는지 확인
                            if (enumElem.ElemInfo.FieldInfos.Length != args.Length)
                            {
                                context.ErrorCollector.Add(exp, "enum인자 개수가 맞지 않습니다");
                                return null;
                            }

                            var argTypeValues = new List<TypeValue>();
                            foreach (var fieldInfo in enumElem.ElemInfo.FieldInfos)
                            {
                                var appliedTypeValue = context.TypeValueService.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
                                argTypeValues.Add(appliedTypeValue);
                            }

                            var nodeInfo = CallExpInfo.MakeEnumValue(enumElem.ElemInfo, argTypeValues);
                            var typeValue = enumElem.EnumTypeValue;

                            return (nodeInfo, typeValue);
                        }
                    }
                }

                return AnalyzeNormal(args);
            }

            (SyntaxNodeInfo NodeInfo, TypeValue TypeValue)? AnalyzeNormal(ImmutableArray<TypeValue> args)
            {
                // 'f'(), 'F'(), 'GetFunc()'()
                if (!AnalyzeCallableExp(exp.Callable, args, context, out var callableInfo))
                    return null;

                var nodeInfo = CallExpInfo.MakeNormal(callableInfo.Value.FuncValue, args);
                var typeValue = callableInfo.Value.TypeValue.Return;
                return (nodeInfo, typeValue);
            }

            // 여기서 분석해야 할 것은 
            // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
            // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
            // 3. 잘 들어갔다면 리턴타입 -> 완료
            outTypeValue = null;            

            if (!AnalyzeExps(exp.Args, context, out var args))
                return false;

            var result = AnalyzeEnumValueOrNormal(args);
            if (result == null)
                return false;

            context.AddNodeInfo(exp, result.Value.NodeInfo);
            outTypeValue = result.Value.TypeValue;
            return true;
        }
        
        internal bool AnalyzeLambdaExp(LambdaExp lambdaExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            if (!analyzer.AnalyzeLambda(lambdaExp.Body, lambdaExp.Params, context, out var captureInfo, out var funcTypeValue, out var localVarCount))
            {
                outTypeValue = null;
                return false;
            }

            outTypeValue = funcTypeValue;
            context.AddNodeInfo(lambdaExp, new LambdaExpInfo(captureInfo, localVarCount));
            return true;
        }

        bool AnalyzeIndexerExp(IndexerExp exp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;

            if (!AnalyzeExp(exp.Object, null, context, out var objTypeValue))
                return false;

            if (!AnalyzeExp(exp.Index, null, context, out var indexTypeValue))
                return false;

            // objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
            if (!context.TypeValueService.GetMemberFuncValue(objTypeValue, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
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

            if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexTypeValue }, context))
                return false;

            context.AddNodeInfo(exp, new IndexerExpInfo(funcValue, objTypeValue, indexTypeValue));

            outTypeValue = funcTypeValue.Return;
            return true;
        }

        // TODO: Hint를 받을 수 있게 해야 한다
        bool AnalyzeExps(IEnumerable<Exp> exps, Context context, out ImmutableArray<TypeValue> outTypeValues)
        {
            var typeValues = new List<TypeValue>();
            foreach (var exp in exps)
            {
                if (!AnalyzeExp(exp, null, context, out var typeValue))
                {
                    outTypeValues = ImmutableArray<TypeValue>.Empty;
                    return false;
                }

                typeValues.Add(typeValue);
            }

            outTypeValues = typeValues.ToImmutableArray();
            return true;
        }

        internal bool AnalyzeMemberCallExp(MemberCallExp exp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;

            var result = new MemberCallExpAnalyzer(this, exp, context).Analyze();
            if (result == null) return false;

            if (!analyzer.CheckParamTypes(exp, result.Value.TypeValue.Params, result.Value.ArgTypeValues, context))
                return false;

            context.AddNodeInfo(exp, result.Value.NodeInfo);
            outTypeValue = result.Value.TypeValue.Return;
            return true;
        }

        internal bool AnalyzeMemberExp(MemberExp memberExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue) 
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

        internal bool AnalyzeListExp(ListExp listExp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            typeValue = null;
            TypeValue? curElemTypeValue = null;

            foreach (var elem in listExp.Elems)
            {
                if (!AnalyzeExp(elem, null, context, out var elemTypeValue))
                    return false;

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

            typeValue = TypeValue.MakeNormal(typeInfos[0].TypeId, TypeArgumentList.Make(curElemTypeValue));
            context.AddNodeInfo(listExp, new ListExpInfo(curElemTypeValue));
            return true;
        }

        public bool AnalyzeExp(Exp exp, TypeValue? hintTypeValue, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            switch(exp)
            {
                case IdentifierExp idExp: return AnalyzeIdExp(idExp, hintTypeValue, context, out typeValue);
                case BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp, context, out typeValue);
                case IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp, context, out typeValue);
                case StringExp stringExp: return AnalyzeStringExp(stringExp, context, out typeValue);
                case UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp, context, out typeValue);
                case BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp, context, out typeValue);
                case CallExp callExp: return AnalyzeCallExp(callExp, hintTypeValue, context, out typeValue);        
                case LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp, context, out typeValue);
                case IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp, context, out typeValue);
                case MemberCallExp memberCallExp: return AnalyzeMemberCallExp(memberCallExp, context, out typeValue);
                case MemberExp memberExp: return AnalyzeMemberExp(memberExp, context, out typeValue);
                case ListExp listExp: return AnalyzeListExp(listExp, context, out typeValue);
                default: throw new NotImplementedException();
            }
        }
    }
}
