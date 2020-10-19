using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static Gum.StaticAnalysis.Analyzer;
using static Gum.StaticAnalysis.Analyzer.Misc;

namespace Gum.StaticAnalysis
{
    partial class ExpAnalyzer
    {
        class UnaryAssignExpAnalyzer : AssignExpAnalyzer
        {
            Analyzer analyzer;
            Context context;
            Syntax.UnaryOpExp exp;

            public UnaryAssignExpAnalyzer(Analyzer analyzer, Context context, Syntax.UnaryOpExp exp)
                : base(analyzer, context)
            {
                this.analyzer = analyzer;
                this.context = context;
                this.exp = exp;
            }

            protected override Result? AnalyzeDirect(TypeValue typeValue, StorageInfo storageInfo)
            {
                // InternalOperator에서 처리할 수 있는지
                if (analyzer.IsAssignable(typeValue, intType, context))
                {
                    var internalOp = GetInternalOp();

                    return new Result(new IR0.CallInternalUnaryAssignOperator(internalOp), typeValue);
                }

                context.ErrorCollector.Add(exp, "int type만 operator++을 사용할 수 있습니다");
                return null;
            }

            private IR0.InternalUnaryAssignOperator GetInternalOp()
            {
                switch (exp.Kind)
                {
                    case Syntax.UnaryOpKind.PostfixDec: return IR0.InternalUnaryAssignOperator.PostfixDec_Int_Int;
                    case Syntax.UnaryOpKind.PostfixInc: return IR0.InternalUnaryAssignOperator.PostfixInc_Int_Int;
                    case Syntax.UnaryOpKind.PrefixDec: return IR0.InternalUnaryAssignOperator.PrefixDec_Int_Int;
                    case Syntax.UnaryOpKind.PrefixInc: return IR0.InternalUnaryAssignOperator.PrefixInc_Int_Int;
                }

                throw new InvalidOperationException();
            }

            private Name GetOperatorName()
            {
                switch (exp.Kind)
                {
                    case Syntax.UnaryOpKind.PostfixDec: return SpecialNames.OpDec;
                    case Syntax.UnaryOpKind.PostfixInc: return SpecialNames.OpInc;
                    case Syntax.UnaryOpKind.PrefixDec: return SpecialNames.OpDec;
                    case Syntax.UnaryOpKind.PrefixInc: return SpecialNames.OpInc;
                }

                throw new InvalidOperationException();
            }

            protected override Result? AnalyzeCall(TypeValue objTypeValue, Syntax.Exp objExp, FuncValue? getter, FuncValue? setter, IEnumerable<(Exp Exp, TypeValue TypeValue)> args)
            {
                // e.x++;
                // e.x가 프로퍼티(GetX, SetX) 라면,
                // let o = Eval(e);
                // let v0 = Eval.Call(o, GetX, [a...]) 
                // let v1 = v0.operator++(); 
                // Eval.Call(o, SetX, [a...]@[v1]) 
                // return v0

                // e.x

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

        //internal bool AnalyzeUnaryAssignExp(
        //    UnaryOpExp unaryOpExp,
        //    Context context,
        //    [NotNullWhen(true)] out IR0.Exp? outExp,
        //    [NotNullWhen(true)] out TypeValue? outTypeValue)
        //{
        //    var assignAnalyzer = new UnaryAssignExpAnalyzer(analyzer, context, unaryOpExp);
        //    return assignAnalyzer.Analyze(out outExp, out outTypeValue);
        //}

    }
}
