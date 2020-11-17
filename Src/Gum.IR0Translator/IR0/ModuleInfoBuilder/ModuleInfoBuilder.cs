using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // Script에서 ModuleInfo 정보를 뽑는 역할
    partial class ModuleInfoBuilder
    {
        TypeExpEvaluator typeExpEvaluator;

        public ModuleInfoBuilder(TypeExpEvaluator typeExpEvaluator)
        {
            this.typeExpEvaluator = typeExpEvaluator;
        }        

        private FuncInfo MakeFunc(
            S.FuncDecl funcDecl,
            ItemId funcId,
            bool bSeqCall,
            bool bThisCall,            
            IEnumerable<string> typeParams,
            TypeValue retTypeValue,
            IEnumerable<TypeValue> argTypeValues,
            Context context)
        {   
            var funcInfo = new FuncInfo(funcId, bSeqCall, bThisCall, typeParams, retTypeValue, argTypeValues);
            context.AddFuncInfo(funcDecl, funcInfo);
            return funcInfo;
        }

        private VarInfo MakeVar(
            ItemId varId,
            bool bStatic,
            TypeValue typeValue,
            Context context)
        {
            var varInfo = new VarInfo(varId, bStatic, typeValue);
            context.AddVarInfo(varInfo);

            return varInfo;
        }

        void BuildTypeDecl(S.TypeDecl typeDecl, Context context)
        {
            switch(typeDecl)
            {
                case S.EnumDecl enumDecl:
                    BuildEnumDecl(enumDecl, context);
                    return;

                case S.StructDecl structDecl:
                    BuildStructDecl(structDecl, context);
                    return;

                default:
                    throw new InvalidOperationException();
            }
        }

        void BuildEnumDecl(S.EnumDecl enumDecl, Context context)
        {
            EnumElemInfo MakeElemInfo(S.EnumDeclElement elem, Context context)
            {
                var fieldInfos = elem.Params.Select(parameter =>
                {
                    var typeValue = context.GetTypeValue(parameter.Type);
                    return new EnumElemFieldInfo(typeValue, parameter.Name);
                });

                return new EnumElemInfo(elem.Name, fieldInfos);
            }
            
            var typePath = context.GetTypePath(enumDecl);
            
            var elemInfos = enumDecl.Elems.Select(elem => MakeElemInfo(elem, context));

            var enumType = new EnumInfo(
                new ItemId(ModuleName.Internal, typePath),                
                enumDecl.TypeParams, 
                elemInfos);

            context.AddTypeInfo(enumDecl, enumType);
        }
        
        void BuildStructDecl(S.StructDecl structDecl, Context context)
        {
            throw new NotImplementedException();
        }

        void BuildFuncDecl(S.FuncDecl funcDecl, Context context)
        {
            var thisTypePath = context.GetThisTypePath();
            var paramHash = GetParamHash(funcDecl.ParamInfo, context);

            ItemId funcId;
            bool bThisCall;
            if (thisTypePath != null)
            {                
                var funcPath = thisTypePath.Value.Append(funcDecl.Name, funcDecl.TypeParams.Length, paramHash);
                funcId = new ItemId(ModuleName.Internal, funcPath);
                bThisCall = true; // TODO: static 키워드가 추가되면 그때 다시 고쳐야 한다
            }
            else
            {
                funcId = new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry(funcDecl.Name, funcDecl.TypeParams.Length, paramHash));
                bThisCall = false;
            }

            MakeFunc(
                funcDecl,
                funcId,
                funcDecl.IsSequence,
                bThisCall,
                funcDecl.TypeParams,
                context.GetTypeValue(funcDecl.RetType),
                funcDecl.ParamInfo.Parameters.Select(typeAndName => context.GetTypeValue(typeAndName.Type)),
                context);
        }        

        private string GetParamHash(S.FuncParamInfo paramInfo, Context context)
        {
            return Misc.MakeParamHash(paramInfo.Parameters.Select(parameter => context.GetTypeValue(parameter.Type)));
        }

        void BuildGlobalStmt(S.Stmt stmt, Context context)
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
        
        void BuildScript(S.Script script, Context context)
        {
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    case S.Script.TypeDeclElement typeDeclElem:
                        BuildTypeDecl(typeDeclElem.TypeDecl, context);
                        break;

                    case S.Script.FuncDeclElement funcElem:
                        BuildFuncDecl(funcElem.FuncDecl, context);
                        break;

                    case S.Script.StmtElement stmtElem:
                        BuildGlobalStmt(stmtElem.Stmt, context);
                        break;
                }
            }
        }

        public Result? BuildScript(IEnumerable<IModuleInfo> moduleInfos, S.Script script, IErrorCollector errorCollector)
        {
            // 2. skeleton과 moduleInfo로 트리의 모든 TypeExp들을 TypeValue로 변환하기            
            var typeEvalResult = typeExpEvaluator.EvaluateScript(script, moduleInfos, errorCollector);
            if (typeEvalResult == null)
                return null;

            var context = new Context(typeEvalResult.Value.SyntaxNodeModuleItemService, typeEvalResult.Value.TypeExpTypeValueService);

            BuildScript(script, context);

            var moduleInfo = new ScriptModuleInfo(
                Array.Empty<NamespaceInfo>(),
                context.GetGlobalItems());

            return new Result(
                moduleInfo,
                typeEvalResult.Value.TypeExpTypeValueService,
                context.GetFuncsByFuncDecl(),
                context.GetTypeInfosByDecl());
        }
    }
}