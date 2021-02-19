using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Misc;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 
        class Context
        {
            ItemValueFactory itemValueFactory;
            GlobalItemValueFactory globalItemValueFactory;

            TypeExpInfoService typeExpTypeValueService;            
            IErrorCollector errorCollector;

            int lambdaCount;

            // 현재 분석되고 있는 함수
            FuncContext curFunc;
            bool bInLoop;
            
            List<R.TypeDecl> typeDecls;
            List<R.FuncDecl> funcDecls;
            List<R.Stmt> topLevelStmts;
            Dictionary<ItemPath, R.FuncDeclId> funcDeclsByPath;

            InternalGlobalVariableRepository internalGlobalVarRepo;

            public Context(
                ItemValueFactory itemValueFactory,
                GlobalItemValueFactory globalItemValueFactory,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector)
            {
                this.itemValueFactory = itemValueFactory;
                this.globalItemValueFactory = globalItemValueFactory;
                this.typeExpTypeValueService = typeExpTypeValueService;
                this.errorCollector = errorCollector;

                lambdaCount = 0;

                curFunc = new FuncContext(null, itemValueFactory.MakeInt(), false);
                bInLoop = false;
                internalGlobalVarRepo = new InternalGlobalVariableRepository();
                
                typeDecls = new List<R.TypeDecl>();
                funcDecls = new List<R.FuncDecl>();
                topLevelStmts = new List<R.Stmt>();
                funcDeclsByPath = new Dictionary<ItemPath, R.FuncDeclId>();
            }

            public void AddLambdaCapture(LambdaCapture lambdaCapture)
            {
                curFunc.AddLambdaCapture(lambdaCapture);
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return curFunc.DoesLocalVarNameExistInScope(name);
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

            public R.ExternalGlobalVarId GetExternalGlobalVarId(ItemId varId)
            {
                throw new NotImplementedException();
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

            public bool IsAssignableExp(R.Exp exp)
            {
                switch (exp)
                {
                    case R.LocalVarExp localVarExp:

                        // 람다 바깥에 있다면 대입 불가능하다
                        if (curFunc.IsLocalVarOutsideLambda(localVarExp.Name))
                            return false;

                        return true;
                    
                    case R.GlobalVarExp _:
                    case R.ListIndexerExp _:
                    case R.StaticMemberExp _:
                    case R.StructMemberExp _:
                    case R.ClassMemberExp _:
                    case R.EnumMemberExp _:
                        return true;

                    default:
                        return false;
                }

            }            

            public void AddInternalGlobalVarInfo(M.Name name, TypeValue typeValue)
            {
                internalGlobalVarRepo.AddInternalGlobalVariable(name, typeValue);
            }

            public void ExecInLocalScope(Action action)
            {
                curFunc.ExecInLocalScope(action);
            }

            public TResult ExecInLocalScope<TResult>(Func<TResult> func)
            {
                return curFunc.ExecInLocalScope(func);                
            }

            public TResult ExecInLoop<TResult>(Func<TResult> func)
            {
                var bPrevInLoop = bInLoop;
                bInLoop = true;

                try
                {
                    return func.Invoke();
                }
                finally
                {
                    bInLoop = bPrevInLoop;
                }
            }

            public void ExecInLoop(Action action)
            {
                var bPrevInLoop = bInLoop;
                bInLoop = true;

                try
                {
                    action.Invoke();
                }
                finally
                {
                    bInLoop = bPrevInLoop;
                }
            }            
            
            public IEnumerable<LocalVarInfo> GetLocalVarsOutsideLambda()
            {
                return curFunc.GetLocalVarsOutsideLambda();
            }

            public void ExecInLambdaScope(TypeValue? lambdaRetTypeValue, Action action)
            {
                curFunc.ExecInLambdaScope(lambdaRetTypeValue, action);
            }

            public void ExecInFuncScope(S.FuncDecl funcDecl, Action action)
            {
                var retTypeValue = GetTypeValueByTypeExp(funcDecl.RetType);
                var funcPath = GetFuncPath(funcDecl);

                var prevFunc = curFunc;
                curFunc = new FuncContext(funcPath, retTypeValue, funcDecl.IsSequence);

                try
                {
                    action.Invoke();
                }
                finally
                {
                    curFunc = prevFunc;
                }
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

            ItemPath GetFuncPath(S.FuncDecl funcDecl)
            {
                throw new NotImplementedException();
            }

            public bool GetTypeValueByName(string varName, [NotNullWhen(true)] out TypeValue? localVarTypeValue)
            {
                throw new NotImplementedException();
            }
            
            public bool IsInLoop()
            {
                return bInLoop;
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

            public LocalVarInfo? GetLocalVarOutsideLambda(string idName)
            {
                return curFunc.GetLocalVarOutsideLambda(idName);
            }

            // 지역 스코프에서 
            public LocalVarInfo? GetLocalVar(string idName)
            {
                return curFunc.GetLocalVarInfo(idName);
            }

            public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName)
            {
                return internalGlobalVarRepo.GetVariable(idName);
            }
            
            public ItemPath? GetCurFuncPath()
            {
                return curFunc.GetFuncPath();
            }

            public void AddNormalFuncDecl(ItemPath itemPath, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<string> paramNames, R.Stmt body)
            {
                var id = new R.FuncDeclId(funcDecls.Count);
                funcDecls.Add(new R.NormalFuncDecl(id, bThisCall, typeParams, paramNames, body));
                funcDeclsByPath.Add(itemPath, id);
            }

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts.Add(stmt);
            }

            
            public IEnumerable<R.TypeDecl> GetTypeDecls()
            {
                return typeDecls;
            }

            public IEnumerable<R.FuncDecl> GetFuncDecls()
            {
                return funcDecls;
            }

            public IEnumerable<R.Stmt> GetTopLevelStmts()
            {
                return topLevelStmts;
            }

            public void AddSequenceFuncDecl(ItemPath itemPath, R.Type retTypeId, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<string> paramNames, R.Stmt body)
            {
                var id = new R.FuncDeclId(funcDecls.Count);
                funcDecls.Add(new R.SequenceFuncDecl(id, retTypeId, bThisCall, typeParams, paramNames,body));
                funcDeclsByPath.Add(itemPath, id);
            }

            public R.CaptureInfo MakeCaptureInfo()
            {
                return curFunc.MakeCaptureInfo();
            }

            // curFunc
            public void AddLocalVarInfo(string name, TypeValue typeValue)
            {
                curFunc.AddLocalVarInfo(name, typeValue);
            }

            public bool IsSeqFunc()
            {
                return curFunc.IsSeqFunc();
            }

            public TypeValue? GetRetTypeValue()
            {
                return curFunc.GetRetTypeValue();
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                curFunc.SetRetTypeValue(retTypeValue);
            }

            public bool DoesInternalGlobalVarNameExist(string name)
            {
                return internalGlobalVarRepo.HasVariable(name);
            }

            public LambdaTypeValue NewLambdaTypeValue(TypeValue retType, IEnumerable<TypeValue> paramTypes)
            {
                int lambdaId = lambdaCount++;
                return itemValueFactory.MakeLambdaType(lambdaId, retType, paramTypes);
            }

            public R.FuncDeclId? GetFuncDeclId(ItemPath funcPath)
            {
                if (funcDeclsByPath.TryGetValue(funcPath, out var funcDeclId))
                    return funcDeclId;
                else
                    return null;
            }

            public ItemResult GetGlobalItem(M.NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                return globalItemValueFactory.GetGlobal(namespacePath, idName, typeArgs, hintTypeValue);
            }

            
        }
    }
}
