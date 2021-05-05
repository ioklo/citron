using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 => TODO: 함수 인자 계산할때 backtracking이 생긴다
        class GlobalContext
        {            
            ItemValueFactory itemValueFactory;
            InternalBinaryOperatorQueryService internalBinOpQueryService;
            GlobalItemValueFactory globalItemValueFactory;

            TypeExpInfoService typeExpTypeValueService;            
            IErrorCollector errorCollector;

            // 현재 분석되고 있는 함수
            List<R.Stmt> topLevelStmts;
            RDeclBuilder declBuilder;

            InternalGlobalVariableRepository internalGlobalVarRepo;

            public GlobalContext(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector)
            {
                this.itemValueFactory = itemValueFactory;
                this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(itemValueFactory);
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpTypeValueService = typeExpTypeValueService;
                this.errorCollector = errorCollector;
                
                internalGlobalVarRepo = new InternalGlobalVariableRepository();

                declBuilder = new GlobalRDeclBuilder();
                topLevelStmts = new List<R.Stmt>();
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, ""));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                errorCollector.Add(new AnalyzeError(code, node, ""));
                throw new FatalAnalyzeException();
            }

            public TypeValue GetBoolType()
            {
                return itemValueFactory.Bool;
            }
            public TypeValue GetIntType()
            {
                return itemValueFactory.Int;
            }

            public TypeValue GetStringType()
            {
                return itemValueFactory.String;
            }

            public TypeValue GetListType(TypeValue elemType)
            {
                return itemValueFactory.List(elemType);
            }

            public bool IsAssignable(TypeValue toTypeValue, TypeValue fromTypeValue)
            {   
                // B <- D
                // 지금은 fromType의 base들을 찾아가면서 toTypeValue와 맞는 것이 있는지 본다
                // TODO: toTypeValue가 interface라면, fromTypeValue의 interface들을 본다

                TypeValue? curType = fromTypeValue;
                while (curType != null)
                {
                    if (toTypeValue.Equals(curType))
                        return true;

                    curType = curType.GetBaseType();
                }

                return false;
            }

            public void AddInternalGlobalVarInfo(M.Name name, TypeValue typeValue)
            {
                internalGlobalVarRepo.AddInternalGlobalVariable(name, typeValue);
            }

            public bool IsBoolType(TypeValue typeValue)
            {
                return itemValueFactory.Bool.Equals(typeValue);
            }

            public bool IsIntType(TypeValue typeValue)
            {
                return itemValueFactory.Int.Equals(typeValue);
            }

            public bool IsStringType(TypeValue typeValue)
            {
                return itemValueFactory.String.Equals(typeValue);
            }

            public TypeValue GetTypeValueByTypeExp(S.TypeExp typeExp)
            {
                var typeExpInfo = typeExpTypeValueService.GetTypeExpInfo(typeExp);

                if (typeExpInfo is MTypeTypeExpInfo mtypeInfo)
                    return itemValueFactory.MakeTypeValue(mtypeInfo.Type);
                else if (typeExpInfo is VarTypeExpInfo)
                    return itemValueFactory.MakeVarTypeValue();
                else
                    throw new UnreachableCodeException();
            }

            public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName)
            {
                return internalGlobalVarRepo.GetVariable(idName);
            }

            public void AddNormalFuncDecl(ImmutableArray<R.LambdaDecl> lambdaDecls, R.Name name, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramNames, R.Stmt body)
            {
                // decls에 할 것이 아니라. curOuter에 해야 한다
                declBuilder.Add(new R.NormalFuncDecl(lambdaDecls, name, bThisCall, typeParams, paramNames, body));
            }

            public void AddSequenceFuncDecl(ImmutableArray<R.LambdaDecl> lambdaDecls, R.Name name, R.Path yieldType, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramInfos, R.Stmt body)
            {
                declBuilder.Add(new R.SequenceFuncDecl(lambdaDecls, name, bThisCall, yieldType, typeParams, paramInfos, body));
            }
            
            public LambdaDecl MakeLambdaDecl(R.Path? capturedThisType, ImmutableArray<R.TypeAndName> captureInfo, ImmutableArray<R.ParamInfo> paramInfos, R.Stmt body)
            {
                var id = new R.DeclId(decls.Count);
                declBuilder.Add(new R.LambdaDecl(id, new R.CapturedStatement(capturedThisType, captureInfo, body), paramInfos));
                return id;
            }

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts.Add(stmt);
            }

            public ImmutableArray<R.Decl> GetDecls() => decls.ToImmutableArray();
            
            public ImmutableArray<R.Stmt> GetTopLevelStmts()
            {
                return topLevelStmts.ToImmutableArray();
            }

            public bool DoesInternalGlobalVarNameExist(string name)
            {
                return internalGlobalVarRepo.HasVariable(name);
            }

            public LambdaTypeValue NewLambdaTypeValue(R.DeclId lambdaDeclId, TypeValue retType, ImmutableArray<TypeValue> paramTypes)
            {
                return itemValueFactory.MakeLambdaType(lambdaDeclId, retType, paramTypes);
            }

            public ItemResult GetGlobalItem(M.NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, ResolveHint hint)
            {
                return globalItemValueFactory.GetGlobal(namespacePath, idName, typeArgs, hint);
            }

            public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(BinaryOpKind kind)
            {
                return internalBinOpQueryService.GetInfos(kind);
            }
        }
    }
}
