using Gum.CompileTime;
using System;
using System.Linq;
using static Gum.IR0Translator.Analyzer;
using static Gum.IR0Translator.Analyzer.Misc;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        //class MemberExpAnalyzer
        //{
        //    public struct Result
        //    {
        //        public MemberExpInfo MemberExpInfo { get;}
        //        public TypeValue TypeValue { get;}
        //        public Result(MemberExpInfo memberExpInfo, TypeValue typeValue)
        //        {
        //            MemberExpInfo = memberExpInfo;
        //            TypeValue = typeValue;
        //        }
        //    }

        //    Analyzer analyzer;
        //    MemberExp memberExp;
        //    Context context;

        //    public MemberExpAnalyzer(Analyzer analyzer, MemberExp memberExp, Context context)
        //    {
        //        this.analyzer = analyzer;
        //        this.memberExp = memberExp;
        //        this.context = context;
        //    }

        //    public Result? Analyze()
        //    {
        //        if (memberExp.Object is IdentifierExp objIdExp)
        //        {
        //            var typeArgs = GetTypeValues(objIdExp.TypeArgs, context);
        //            if (!context.GetIdentifierInfo(objIdExp.Value, typeArgs, null, out var idInfo))
        //                return null;

        //            if (idInfo is IdentifierInfo.Type typeIdInfo)
        //                return Analyze_EnumOrType(typeIdInfo.TypeValue); 
        //        }
                
        //        if (!analyzer.AnalyzeExp(memberExp.Object, null, context, out var objTypeValue))
        //            return null;

        //        return Analyze_EnumElemOrInstance(objTypeValue);
        //    }

        //    private Result? Analyze_EnumElemOrInstance(TypeValue objTypeValue)
        //    {
        //        // enumElem의 경우
        //        if (objTypeValue is EnumElemTypeValue enumElem)
        //        {
        //            var enumInfo = (IEnumInfo)context.ModuleInfoService.GetTypeInfos(enumElem.EnumTypeValue.TypeId).Single();
        //            if (!enumInfo.GetElemInfo(enumElem.Name, out var elemInfo))
        //            {
        //                context.ErrorCollector.Add(memberExp, $"{memberExp.MemberName}은 {enumInfo}의 멤버가 아닙니다");
        //                return null;
        //            }
                    
        //            foreach(var fieldInfo in elemInfo.Value.FieldInfos)
        //            {
        //                if (fieldInfo.Name == memberExp.MemberName)
        //                {
        //                    var nodeInfo = MemberExpInfo.MakeEnumElemField(enumElem.EnumTypeValue, memberExp.MemberName);
        //                    var typeValue = context.TypeValueService.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);

        //                    return new Result(nodeInfo, typeValue);
        //                }
        //            }

        //            return null;
        //        }

        //        return Analyze_Instance(objTypeValue);
        //    }

        //    private Result? Analyze_Instance(TypeValue objTypeValue)
        //    {
        //        if (!analyzer.CheckInstanceMember(memberExp, objTypeValue, context, out var varValue))
        //            return null;

        //        // instance이지만 static 이라면, exp는 실행하고, static변수에서 가져온다
        //        var nodeInfo = IsVarStatic(varValue.VarId, context)
        //            ? MemberExpInfo.MakeStatic(objTypeValue, varValue)
        //            : MemberExpInfo.MakeInstance(objTypeValue, memberExp.MemberName);

        //        var typeValue = context.TypeValueService.GetTypeValue(varValue);

        //        return new Result(nodeInfo, typeValue);
        //    }

        //    private Result? Analyze_EnumOrType(NormalTypeValue objNTV)
        //    {
        //        var typeInfo = context.ModuleInfoService.GetTypeInfos(objNTV.TypeId).Single();
        //        if (typeInfo is IEnumInfo enumTypeInfo)
        //        {
        //            if (!enumTypeInfo.GetElemInfo(memberExp.MemberName, out var elemInfo))
        //                return null;
                    
        //            if (elemInfo.Value.FieldInfos.Length == 0)
        //            {
        //                var nodeInfo = MemberExpInfo.MakeEnumElem(objNTV, memberExp.MemberName);
        //                var typeValue = objNTV;
                         
        //                return new Result(nodeInfo, typeValue);
        //            }
        //            else
        //            {
        //                // TODO: FieldInfo가 있을 경우 함수로 감싸기
        //                throw new NotImplementedException();
        //            }
        //        }

        //        return Analyze_Type(objNTV);
        //    }

        //    private Result? Analyze_Type(NormalTypeValue objNTV)
        //    {
        //        if (!analyzer.CheckStaticMember(memberExp, objNTV, context, out var varValue))
        //            return null;

        //        var nodeInfo = MemberExpInfo.MakeStatic(null, varValue);
        //        var typeValue = context.TypeValueService.GetTypeValue(varValue);

        //        return new Result(nodeInfo, typeValue);
        //    }
        //}
    }
}
