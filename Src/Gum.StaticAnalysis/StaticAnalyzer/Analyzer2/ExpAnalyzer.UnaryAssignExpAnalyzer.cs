using Gum.CompileTime;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static Gum.StaticAnalysis.Analyzer2;
using static Gum.StaticAnalysis.Analyzer2.Misc;

namespace Gum.StaticAnalysis
{
    partial class ExpAnalyzer2
    {
        class UnaryAssignExpAnalyzer : AssignExpAnalyzer
        {
            Analyzer2 analyzer;
            Context context;
            UnaryOpExp exp;

            public UnaryAssignExpAnalyzer(Analyzer2 analyzer, Context context, UnaryOpExp exp)
                : base(analyzer, context)
            {
                this.analyzer = analyzer;
                this.context = context;
                this.exp = exp;
            }

            protected override TypeValue? AnalyzeDirect(TypeValue typeValue, StorageInfo storageInfo)
            {
                var operatorName = GetOperatorName();

                if (!context.TypeValueService.GetMemberFuncValue(typeValue, operatorName, Array.Empty<TypeValue>(), out var operatorValue))
                {
                    context.ErrorCollector.Add(exp, "해당 타입에 operator++이 없습니다");
                    return null;
                }

                if (!IsFuncStatic(operatorValue.FuncId, context))
                {
                    context.ErrorCollector.Add(exp, "operator++은 static이어야 합니다");
                    return null;
                }

                var bReturnPrevValue = ShouldReturnPrevValue();
                context.AddNodeInfo(exp, UnaryOpExpAssignInfo.MakeDirect(storageInfo, operatorValue, bReturnPrevValue, typeValue));

                return typeValue;
            }

            private bool ShouldReturnPrevValue()
            {
                switch (exp.Kind)
                {
                    case UnaryOpKind.PostfixDec:
                    case UnaryOpKind.PostfixInc:
                        return true;

                    case UnaryOpKind.PrefixDec:
                    case UnaryOpKind.PrefixInc:
                        return false;
                }

                throw new InvalidOperationException();
            }

            private Name GetOperatorName()
            {
                switch (exp.Kind)
                {
                    case UnaryOpKind.PostfixDec: return SpecialNames.OpDec;
                    case UnaryOpKind.PostfixInc: return SpecialNames.OpInc;
                    case UnaryOpKind.PrefixDec: return SpecialNames.OpDec;
                    case UnaryOpKind.PrefixInc: return SpecialNames.OpInc;
                }

                throw new InvalidOperationException();
            }

            protected override TypeValue? AnalyzeCall(TypeValue objTypeValue, Exp objExp, FuncValue? getter, FuncValue? setter, IEnumerable<(Exp Exp, TypeValue TypeValue)> args)
            {
                // e.x++;
                // e.x가 프로퍼티(GetX, SetX) 라면,
                // let o = Eval(e);
                // let v0 = Eval.Call(o, GetX, [a...]) 
                // let v1 = v0.operator++(); 
                // Eval.Call(o, SetX, [a...]@[v1]) 
                // return v0

                if (getter == null || setter == null)
                {
                    context.ErrorCollector.Add(objExp, "getter, setter 모두 존재해야 합니다");
                    return null;
                }

                // 1. getter의 인자 타입이 args랑 맞는지
                // 2. getter의 리턴 타입이 operator++을 지원하는지,
                // 3. setter의 인자 타입이 {getter의 리턴타입의 operator++의 리턴타입}을 포함해서 args와 맞는지
                // 4. 이 expression의 타입은 getter의 리턴 타입

                var argTypeValues = args.Select(arg => arg.TypeValue).ToList();

                // 1.
                var getterTypeValue = context.TypeValueService.GetTypeValue(getter);
                if (!analyzer.CheckParamTypes(objExp, getterTypeValue.Params, argTypeValues, context))
                    return null;

                // 2. 
                var operatorName = GetOperatorName();
                if (!context.TypeValueService.GetMemberFuncValue(getterTypeValue.Return, operatorName, Array.Empty<TypeValue>(), out var operatorValue))
                {
                    context.ErrorCollector.Add(objExp, $"{objExp}에서 {operatorName} 함수를 찾을 수 없습니다");
                    return null;
                }
                var operatorTypeValue = context.TypeValueService.GetTypeValue(operatorValue);

                // 3. 
                argTypeValues.Add(operatorTypeValue.Return);
                var setterTypeValue = context.TypeValueService.GetTypeValue(setter);
                if (!analyzer.CheckParamTypes(objExp, setterTypeValue.Params, argTypeValues, context))
                    return null;

                // TODO: QsPropertyInfo 가 만들어진다면 위의 1, 2, 3, 4는 프로퍼티와 operator의 작성할 때 Constraint일 것이므로 위의 체크를 건너뛰어도 된다. Prop
                // 1. getter, setter의 인자타입은 동일해야 한다
                // 2. getter의 리턴타입은 setter의 마지막 인자와 같아야 한다
                // 3. {T}.operator++은 {T} 타입만 리턴해야 한다                
                // 4. 이 unaryExp의 타입은 프로퍼티의 타입이다

                context.AddNodeInfo(exp, UnaryOpExpAssignInfo.MakeCallFunc(
                    objExp, objTypeValue,
                    getterTypeValue.Return,
                    operatorTypeValue.Return,
                    ShouldReturnPrevValue(),
                    args,
                    getter, setter, operatorValue));

                return operatorTypeValue;
            }

            protected override Exp GetTargetExp()
            {
                return exp.Operand;
            }
        }

        internal bool AnalyzeUnaryAssignExp(
            UnaryOpExp unaryOpExp,
            Context context,
            [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            var assignAnalyzer = new UnaryAssignExpAnalyzer(analyzer, context, unaryOpExp);
            return assignAnalyzer.Analyze(out outTypeValue);
        }

    }
}
