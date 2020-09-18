using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Gum.StaticAnalysis
{
    public partial class ModuleInfoBuilder
    {
        TypeExpEvaluator typeExpEvaluator;

        public ModuleInfoBuilder(TypeExpEvaluator typeExpEvaluator)
        {
            this.typeExpEvaluator = typeExpEvaluator;
        }        

        private FuncInfo MakeFunc(
            FuncDecl? funcDecl,
            ModuleItemId? outerId,
            ModuleItemId funcId,
            bool bSeqCall,
            bool bThisCall,            
            IEnumerable<string> typeParams,
            TypeValue retTypeValue,
            IEnumerable<TypeValue> argTypeValues,
            Context context)
        {   
            var funcInfo = new FuncInfo(outerId, funcId, bSeqCall, bThisCall, typeParams, retTypeValue, argTypeValues);
            context.AddFuncInfo(funcDecl, funcInfo);
            return funcInfo;
        }

        private VarInfo MakeVar(
            ModuleItemId? outerId,
            ModuleItemId varId,
            bool bStatic,
            TypeValue typeValue,
            Context context)
        {
            var varInfo = new VarInfo(outerId, varId, bStatic, typeValue);
            context.AddVarInfo(varInfo);

            return varInfo;
        }

        void BuildEnumDecl(EnumDecl enumDecl, Context context)
        {
            EnumElemInfo MakeElemInfo(ModuleItemId enumTypeId, EnumDeclElement elem, Context context)
            {
                var fieldInfos = elem.Params.Select(parameter =>
                {
                    var typeValue = context.GetTypeValue(parameter.Type);
                    return new EnumElemFieldInfo(typeValue, parameter.Name);
                });

                return new EnumElemInfo(elem.Name, fieldInfos);
            }
            
            var typeId = context.GetTypeId(enumDecl);
            
            var elemInfos = enumDecl.Elems.Select(elem => MakeElemInfo(typeId, elem, context));

            var enumType = new EnumInfo(
                context.GetThisTypeValue()?.TypeId,
                typeId,
                enumDecl.TypeParams, elemInfos);

            context.AddEnumInfo(enumDecl, enumType);
        }

        void BuildFuncDecl(FuncDecl funcDecl, Context context)
        {
            var thisTypeValue = context.GetThisTypeValue();            

            ModuleItemId? outerId = null;
            bool bThisCall = false;

            if (thisTypeValue != null)
            {
                outerId = thisTypeValue.TypeId;
                bThisCall = true; // TODO: static 키워드가 추가되면 그때 다시 고쳐야 한다
            }

            var func = MakeFunc(
                funcDecl,
                outerId,                
                context.GetFuncId(funcDecl),
                funcDecl.IsSequence,
                bThisCall,
                funcDecl.TypeParams,
                context.GetTypeValue(funcDecl.RetType),
                funcDecl.ParamInfo.Parameters.Select(typeAndName => context.GetTypeValue(typeAndName.Type)),
                context);
        }

        void BuildGlobalStmt(Stmt stmt, Context context)
        {
            // TODO: public int x; 형식만 모듈단위 외부 전역변수로 노출시킨다

            //var varDeclStmt = stmt as QsVarDeclStmt;
            //if (varDeclStmt == null) return;

            //var typeValue = context.TypeValuesByTypeExp[varDeclStmt.VarDecl.Type];
            
            //foreach(var elem in varDeclStmt.VarDecl.Elems)
            //{
            //    // TODO: 인자 bStatic에 true/false가 아니라, Global이라고 체크를 해야 한다
            //    MakeVar(QsMetaItemId.Make(new QsMetaItemIdElem(elem.VarName)), bStatic: true, typeValue, context);
            //}
        }
        
        void BuildScript(Script script, Context context)
        {
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    case Script.EnumDeclElement enumElem:
                        BuildEnumDecl(enumElem.EnumDecl, context);
                        break;

                    case Script.FuncDeclElement funcElem:
                        BuildFuncDecl(funcElem.FuncDecl, context);
                        break;

                    case Script.StmtElement stmtElem:
                        BuildGlobalStmt(stmtElem.Stmt, context);
                        break;
                }
            }
        }

        public Result? BuildScript(string moduleName, IEnumerable<IModuleInfo> moduleInfos, Script script, IErrorCollector errorCollector)
        {
            // 2. skeleton과 moduleInfo로 트리의 모든 TypeExp들을 TypeValue로 변환하기            
            var typeEvalResult = typeExpEvaluator.EvaluateScript(script, moduleInfos, errorCollector);
            if (typeEvalResult == null)
                return null;

            var context = new Context(typeEvalResult.Value.SyntaxNodeModuleItemService, typeEvalResult.Value.TypeExpTypeValueService);

            BuildScript(script, context);

            var moduleInfo = new ScriptModuleInfo(
                moduleName, 
                context.GetTypeInfos(), 
                context.GetFuncInfos(), 
                context.GetVarInfos());

            return new Result(
                moduleInfo,
                typeEvalResult.Value.TypeExpTypeValueService,
                context.GetFuncsByFuncDecl(),
                context.GetEnumInfosByDecl());
        }
    }
}