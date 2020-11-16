using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using static Gum.IR0.Analyzer;
using static Gum.IR0.Analyzer.Misc;
using Gum.Infra;

namespace Gum.IR0
{
    partial class ExpAnalyzer
    {
        //class MemberCallExpAnalyzer
        //{
        //    public struct Result
        //    {
        //        public TypeValue.Func TypeValue { get; }
        //        public IR0.Exp Exp { get; }                

        //        public Result(TypeValue.Func typeValue, IR0.Exp exp)
        //        {
        //            TypeValue = typeValue;
        //            Exp = exp;
        //        }
        //    }

        //    Analyzer analyzer;
        //    Syntax.MemberCallExp exp;
        //    Context context;

        //    public MemberCallExpAnalyzer(Analyzer analyzer, Syntax.MemberCallExp exp, Context context)
        //    {
        //        this.analyzer = analyzer;
        //        this.exp = exp;
        //        this.context = context;
        //    }

        //    public Result? Analyze()
        //    {
        //        // id인 경우는 따로 처리
        //        if (exp.Object is Syntax.IdentifierExp objIdExp)
        //        {
        //            return AnalyzeObjectIdExp(objIdExp);
        //        }
        //        else
        //        {
        //            if (!analyzer.AnalyzeExp(exp.Object, null, context, out var ir0Exp, out var objTypeValue))
        //                return null;

        //            return Analyze_Instance(ir0Exp, objTypeValue);
        //        }
        //    }

        //    private Result? AnalyzeObjectIdExp(Syntax.IdentifierExp objIdExp)
        //    {
        //        var typeArgs = GetTypeValues(objIdExp.TypeArgs, context);

        //        if (!context.GetIdentifierInfo(objIdExp.Value, typeArgs, null, out var idInfo))
        //            return null;

        //        if (idInfo is IdentifierInfo.Type typeIdInfo)
        //        {
        //            return Analyze_EnumOrType(typeIdInfo);
        //        }
        //        else if (idInfo is IdentifierInfo.Func funcIdInfo)
        //        {
        //            // ? 'Func'.m(...) 꼴 은 허용을 하지 않는다
        //            context.ErrorCollector.Add(exp, $"{objIdExp.Value}가 함수입니다. 함수는 멤버를 가질 수 없습니다.");
        //            return null;
        //        }
        //        else if (idInfo is IdentifierInfo.Var varIdInfo)
        //        {
        //            if (!analyzer.AnalyzeExp(exp.Object, null, context, out var ir0Exp, out var objTypeValue))
        //                return null;

        //            // 두번 계산;
        //            Debug.Assert(objTypeValue == varIdInfo.TypeValue);

        //            return Analyze_Instance(ir0Exp, objTypeValue);
        //        }

        //        throw new InvalidOperationException();
        //    }

        //    private Result? Analyze_Instance(IR0.Exp instanceExp, TypeValue instanceType)
        //    {
        //        var memberTypeArgs = GetTypeValues(exp.MemberTypeArgs, context);

        //        // 1. 함수에서 찾기.. FuncValue도 같이 주는것이 좋을 듯 하다
        //        if (context.TypeValueService.GetMemberFuncValue(instanceType, exp.MemberName, memberTypeArgs, out var funcValue))
        //        {
        //            var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

        //            if (!analyzer.AnalyzeExps(exp.Args, context, out var argInfos))
        //                return null;

        //            if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, argInfos.Select(argInfo => argInfo.TypeValue).ToList(), context))
        //                return null;

        //            var ir0Exp = new IR0.CallFuncExp(funcValue, (instanceExp, instanceType), argInfos.Select(argInfo => new IR0.ExpInfo(argInfo.Exp, argInfo.TypeValue)));
        //            return new Result(funcTypeValue, ir0Exp);
        //        }

        //        // 2. 변수에서 찾기
        //        if (memberTypeArgs.Length == 0)
        //        {
        //            if (context.TypeValueService.GetMemberVarValue(instanceType, exp.MemberName, out var varValue))
        //            {
        //                // TODO: as 대신 함수로 의미 부여하기, 호출 가능하면? 쿼리하는 함수로 변경
        //                var varFuncTypeValue = context.TypeValueService.GetTypeValue(varValue) as TypeValue.Func;

        //                if (varFuncTypeValue == null)
        //                {
        //                    context.ErrorCollector.Add(exp, $"호출 가능한 타입이 아닙니다");
        //                    return null;
        //                }

        //                if (!analyzer.AnalyzeExps(exp.Args, context, out var argInfos))
        //                    return null;

        //                if (!analyzer.CheckParamTypes(exp, varFuncTypeValue.Params, argInfos.Select(argInfo => argInfo.TypeValue).ToList(), context))
        //                    return null;

        //                var callableExp = new IR0.MemberValueExp(instanceExp, exp.MemberName);
        //                var ir0Exp = new IR0.CallValueExp(callableExp, varFuncTypeValue, argInfos.Select(argInfo => new IR0.ExpInfo(argInfo.Exp, argInfo.TypeValue)));

        //                return new Result(varFuncTypeValue, ir0Exp);
        //            }
        //        }

        //        // 변수에서 찾기 VarId도 같이 주는것이 좋을 것 같다
        //        context.ErrorCollector.Add(exp, $"{exp.Object}에 {exp.MemberName} 함수가 없습니다");
        //        return null;
        //    }

        //    private Result? Analyze_EnumOrType(IdentifierInfo.Type typeIdInfo)
        //    {
        //        var typeInfo = context.ModuleInfoService.GetTypeInfos(typeIdInfo.TypeValue.TypeId).Single();
        //        if (typeInfo is IEnumInfo enumTypeInfo)
        //        {
        //            // E<T>.Second<S> 는 허용하지 않음
        //            if (exp.MemberTypeArgs.Length != 0)
        //            {
        //                context.ErrorCollector.Add(exp, "enum 생성자는 타입인자를 가질 수 없습니다");
        //                return null;
        //            }

        //            // 'First'로 E<T>.First 정보 찾기
        //            if (!enumTypeInfo.GetElemInfo(exp.MemberName, out var elemInfo))
        //            {
        //                context.ErrorCollector.Add(exp, $"{exp.MemberName}에 해당하는 enum 생성자를 찾을 수 없습니다");
        //                return null;
        //            }
                    
        //            // E<T>.First 인 경우, 인자를 받지 않으므로 에러
        //            if (elemInfo.Value.FieldInfos.Length == 0)
        //            {
        //                context.ErrorCollector.Add(exp, $"{exp.MemberName} enum 값은 인자를 받지 않습니다");
        //                return null;
        //            }

        //            // E<T>.Second(int i, T t) 에서 (int, T)가져오기
        //            var paramTypes = elemInfo.Value.FieldInfos.Select(fieldInfo => fieldInfo.TypeValue).ToList();
        //            var funcTypeValue = TypeValue.MakeFunc(typeIdInfo.TypeValue, paramTypes);
        //            var appliedFuncTypeValue = context.TypeValueService.Apply(typeIdInfo.TypeValue, funcTypeValue);

        //            var members = new List<IR0.NewEnumExp.Elem>();

        //            if (!analyzer.AnalyzeExps(exp.Args, context, out var argInfos))
        //                return null;

        //            if (!analyzer.CheckParamTypes(exp, paramTypes, argInfos.Select(argInfo => argInfo.TypeValue).ToList(), context))
        //                return null;

        //            Enumerable.Zip(elemInfo.Value.FieldInfos, argInfos, (fieldInfo, argInfo) => 
        //                new IR0.NewEnumExp.Elem(fieldInfo.Name, new IR0.ExpInfo(argInfo.Exp, argInfo.TypeValue)));

        //            foreach (var (fieldInfo, argInfos) in Zip(elemInfo.Value.FieldInfos, argInfos))
        //            {

        //            }
        //            var nodeInfo = MemberCallExpInfo.MakeEnumValue(null, args, elemInfo.Value);

        //            // var ir0Exp = new IR0.NewEnumExp(elemInfo.Value.Name, );

        //            throw new NotImplementedException();

        //            return new Result(funcTypeValue, ir0Exp);
        //        }

        //        return Analyze_Type(typeIdInfo);
        //    }

        //    private Result? Analyze_Type(IdentifierInfo.Type typeIdInfo)
        //    {
        //        var objTypeValue = typeIdInfo.TypeValue;
        //        var memberTypeArgs = GetTypeValues(exp.MemberTypeArgs, context);

        //        // 1. 함수에서 찾기
        //        if (context.TypeValueService.GetMemberFuncValue(objTypeValue, exp.MemberName, memberTypeArgs, out var memberFuncValue))
        //        {
        //            if (!IsFuncStatic(memberFuncValue.FuncId, context))
        //            {
        //                context.ErrorCollector.Add(exp, "정적 함수만 호출할 수 있습니다");
        //                return null;
        //            }

        //            // staticObject인 경우는 StaticFunc만, 아니라면 모든 함수가 가능 
        //            var funcTypeValue = context.TypeValueService.GetTypeValue(memberFuncValue);
        //            var nodeInfo = MemberCallExpInfo.MakeStaticFunc(null, args, memberFuncValue);

        //            return new Result(funcTypeValue, nodeInfo, args);
        //        }

        //        // 2. 변수에서 찾기
        //        if (memberTypeArgs.Length == 0)
        //        {
        //            if (context.TypeValueService.GetMemberVarValue(objTypeValue, exp.MemberName, out var varValue))
        //            {
        //                if (!IsVarStatic(varValue.VarId, context))
        //                {
        //                    context.ErrorCollector.Add(exp, "정적 변수만 참조할 수 있습니다");
        //                    return null;
        //                }

        //                // TODO: as 대신 함수로 의미 부여하기, 호출 가능하면? 쿼리하는 함수로 변경
        //                var varFuncTypeValue = context.TypeValueService.GetTypeValue(varValue) as TypeValue.Func;

        //                if (varFuncTypeValue == null)
        //                {
        //                    context.ErrorCollector.Add(exp, $"호출 가능한 타입이 아닙니다");
        //                    return null;
        //                }

        //                return new Result(varFuncTypeValue, MemberCallExpInfo.MakeStaticLambda(null, args, varValue), args);
        //            }
        //        }

        //        // 변수에서 찾기 VarId도 같이 주는것이 좋을 것 같다
        //        context.ErrorCollector.Add(exp, $"{exp.Object}에 {exp.MemberName} 함수가 없습니다");
        //        return null;
        //    }
        //}
    }
}
