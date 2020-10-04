using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using static Gum.StaticAnalysis.Analyzer;
using static Gum.StaticAnalysis.Analyzer.Misc;
using Gum.Syntax;
using Gum.CompileTime;

namespace Gum.StaticAnalysis
{
    partial class ExpAnalyzer
    {
        class BinaryAssignExpAnalyzer : AssignExpAnalyzer
        {
            Analyzer analyzer;
            Context context;
            BinaryOpExp exp;

            public BinaryAssignExpAnalyzer(Analyzer analyzer, BinaryOpExp exp, Context context)
                : base(analyzer, context)
            {
                this.analyzer = analyzer;
                this.context = context;
                this.exp = exp;
            }

            protected override Exp GetTargetExp()
            {
                return exp.Operand0;
            }

            protected override TypeValue? AnalyzeDirect(TypeValue typeValue0, StorageInfo storageInfo)
            {
                // operand1 검사
                if (!analyzer.AnalyzeExp(exp.Operand1, typeValue0, context, out var typeValue1))
                    return null;

                if (!analyzer.IsAssignable(typeValue0, typeValue1, context))
                {
                    context.ErrorCollector.Add(exp, $"{typeValue1}를 {typeValue0}에 대입할 수 없습니다");
                    return null;
                }

                var nodeInfo = BinaryOpExpAssignInfo.MakeDirect(storageInfo);

                context.AddNodeInfo(exp, nodeInfo);
                return typeValue1;
            }

            protected override TypeValue? AnalyzeCall(
                TypeValue objTypeValue,
                Exp objExp,
                FuncValue? getter,
                FuncValue? setter,
                IEnumerable<(Exp Exp, TypeValue TypeValue)> args)
            {
                // setter만 쓴다
                if (setter == null)
                {
                    context.ErrorCollector.Add(objExp, "객체에 setter함수가 없습니다");
                    return null;
                }

                if (IsFuncStatic(setter.FuncId, context))
                {
                    context.ErrorCollector.Add(objExp, "객체의 setter는 static일 수 없습니다");
                    return null;
                }

                var setterTypeValue = context.TypeValueService.GetTypeValue(setter);

                if (!analyzer.AnalyzeExp(exp.Operand1, null, context, out var operandTypeValue1))
                    return null;

                var setterArgTypeValues = args.Select(a => a.TypeValue).Append(operandTypeValue1).ToList();
                if (!analyzer.CheckParamTypes(objExp, setterTypeValue.Params, setterArgTypeValues, context))
                    return null;

                if (setterTypeValue.Return != TypeValue.MakeVoid())
                {
                    context.ErrorCollector.Add(objExp, "setter는 void를 반환해야 합니다");
                    return null;
                }

                var nodeInfo = BinaryOpExpAssignInfo.MakeCallSetter(
                    objTypeValue,
                    objExp,
                    setter,
                    args,
                    operandTypeValue1);

                context.AddNodeInfo(exp, nodeInfo);
                return operandTypeValue1;
            }
        }

        // 두가지 
        // X.m = e;
        // e1.m = e2;
        // 요구사항 
        // 1. NodeInfo에 Normal, CallSetter를 넣는다
        // 2. Normal중 MemberExpInfo라면 QsMemberExpInfo를 붙이고, IdExpInfo라면 QsIdentifierExpInfo를 붙인다. =? 이건 기존의 Analyzer에 넣으면 될듯
        // 3. Normal의 나머지 케이스에 대해서는 에러를 낸다
        // 4. CallSetter는 일단 Indexer일때만 
        internal bool AnalyzeBinaryAssignExp(
            BinaryOpExp binaryOpExp,
            Context context,
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            var assignAnalyzer = new BinaryAssignExpAnalyzer(analyzer, binaryOpExp, context);
            return assignAnalyzer.Analyze(out outTypeValue);
        }

    }
}
