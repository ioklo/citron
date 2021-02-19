using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Gum.IR0Translator.Analyzer;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {   
        //abstract class AssignExpAnalyzer
        //{
        //    protected struct Result
        //    {
        //        public IR0.Exp Exp { get; }
        //        public TypeValue Type { get; }
        //        public Result(IR0.Exp exp, TypeValue type)
        //        {
        //            Exp = exp;
        //            Type = type;
        //        }
        //    }

        //    Analyzer analyzer;
        //    Context context;

        //    public AssignExpAnalyzer(Analyzer analyzer, Context context)
        //    {
        //        this.analyzer = analyzer;
        //        this.context = context;
        //    }

        //    protected abstract Syntax.Exp GetTargetExp();
        //    protected abstract Result? AnalyzeDirect(TypeValue typeValue, StorageInfo storageInfo);
        //    protected abstract Result? AnalyzeCall(
        //        TypeValue objTypeValue,
        //        Syntax.Exp objExp,
        //        FuncValue? getter,
        //        FuncValue? setter,
        //        IEnumerable<(Syntax.Exp Exp, TypeValue TypeValue)> args);

        //    Result? AnalyzeAssignToIdExp(Syntax.IdentifierExp idExp)
        //    {
        //        var typeArgs = GetTypeValues(idExp.TypeArgs, context);

        //        if (!context.GetIdentifierInfo(idExp.Value, typeArgs, null, out var idInfo))
        //            return null;

        //        if (idInfo is IdentifierInfo.Var varIdInfo)
        //            return AnalyzeDirect(varIdInfo.TypeValue, varIdInfo.StorageInfo);

        //        // TODO: Func
        //        return null;
        //    }

        //    Result? AnalyzeAssignToMemberExp(Syntax.MemberExp memberExp)
        //    {
        //        // i.m = e1
        //        if (memberExp.Object is Syntax.IdentifierExp objIdExp)
        //        {
        //            var typeArgs = GetTypeValues(objIdExp.TypeArgs, context);
        //            if (!context.GetIdentifierInfo(objIdExp.Value, typeArgs, null, out var idInfo))
        //                return null;

        //            // X.m = e1
        //            if (idInfo is IdentifierInfo.Type typeIdInfo)
        //                return AnalyzeAssignToStaticMember(memberExp, typeIdInfo.TypeValue);
        //        }

        //        if (!analyzer.AnalyzeExp(memberExp.Object, null, context, out var objTypeValue))
        //            return null;

        //        return AnalyzeAssignToInstanceMember(memberExp, objTypeValue);
        //    }

        //    Result? AnalyzeAssignToStaticMember(Syntax.MemberExp memberExp, NormalTypeValue objNormalTypeValue)
        //    {
        //        if (!analyzer.CheckStaticMember(memberExp, objNormalTypeValue, context, out var varValue))
        //            return null;

        //        var typeValue = context.TypeValueService.GetTypeValue(varValue);

        //        return AnalyzeDirect(typeValue, StorageInfo.MakeStaticMember(null, varValue));
        //    }

        //    Result? AnalyzeAssignToInstanceMember(Syntax.MemberExp memberExp, TypeValue objTypeValue)
        //    {
        //        if (!analyzer.CheckInstanceMember(memberExp, objTypeValue, context, out var varValue))
        //            return null;

        //        var typeValue = context.TypeValueService.GetTypeValue(varValue);

        //        // instance이지만 static 이라면, exp는 실행하고, static변수에서 가져온다
        //        StorageInfo storageInfo;
        //        if (IsVarStatic(varValue.VarId, context))
        //            storageInfo = StorageInfo.MakeStaticMember((objTypeValue, memberExp.Object), varValue);
        //        else
        //            storageInfo = StorageInfo.MakeInstanceMember(memberExp.Object, objTypeValue, memberExp.MemberName));

        //        return AnalyzeDirect(typeValue, storageInfo);
        //    }

        //    Result? AnalyzeAssignToIndexerExp(Syntax.IndexerExp indexerExp0)
        //    {
        //        if (!analyzer.AnalyzeExp(indexerExp0.Object, null, context, out var objTypeValue))
        //            return null;

        //        if (!analyzer.AnalyzeExp(indexerExp0.Index, null, context, out var indexTypeValue))
        //            return null;

        //        context.TypeValueService.GetMemberFuncValue(
        //            objTypeValue,
        //            SpecialNames.IndexerSet,
        //            Array.Empty<TypeValue>(),
        //            out var setter);

        //        context.TypeValueService.GetMemberFuncValue(
        //            objTypeValue,
        //            SpecialNames.IndexerGet,
        //            Array.Empty<TypeValue>(),
        //            out var getter);

        //        return AnalyzeCall(objTypeValue, indexerExp0.Object, getter, setter, new[] { (indexerExp0.Index, indexTypeValue) });
        //    }

        //    public bool Analyze([NotNullWhen(true)] out IR0.Exp? outExp, [NotNullWhen(true)] out TypeValue? outTypeValue)
        //    {
        //        var locExp = GetTargetExp();

        //        Result? result;
        //        // x = e1, x++
        //        if (locExp is Syntax.IdentifierExp idExp)
        //        {
        //            result = AnalyzeAssignToIdExp(idExp);
        //        }
        //        // eo.m = e1, eo.m++
        //        else if (locExp is Syntax.MemberExp memberExp)
        //        {
        //            result = AnalyzeAssignToMemberExp(memberExp);
        //        }
        //        // eo[ei] = e1
        //        else if (locExp is Syntax.IndexerExp indexerExp)
        //        {
        //            result = AnalyzeAssignToIndexerExp(indexerExp);
        //        }
        //        else
        //        {
        //            context.ErrorCollector.Add(locExp, "식별자, 멤버 변수, 멤버 프로퍼티, 인덱서 에만 대입할 수 있습니다");
        //            result = null;
        //        }

        //        if (result == null)
        //        {
        //            outExp = null;
        //            outTypeValue = null;
        //            return false;
        //        }

        //        outExp = result.Value.Exp;
        //        outTypeValue = result.Value.Type;
        //        return true;
        //    }
        //}
    }
}