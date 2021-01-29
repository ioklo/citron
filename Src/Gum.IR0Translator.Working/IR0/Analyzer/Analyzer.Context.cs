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

namespace Gum.IR0
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 
        public class Context
        {
            public TItemInfo? GetItem<TItemInfo>(ItemId id)
                where TItemInfo : ItemInfo
            {
                return itemInfoRepo.GetItem<TItemInfo>(id);
            }

            M.ModuleInfo internalModuleInfo;

            ItemInfoRepository itemInfoRepo;
            TypeExpInfoService typeExpTypeValueService;
            TypeValueService typeValueService;
            IErrorCollector errorCollector;

            // 현재 분석되고 있는 함수
            FuncContext curFunc;
            bool bInLoop;
            
            List<TypeDecl> typeDecls;
            List<FuncDecl> funcDecls;
            List<Stmt> topLevelStmts;
            Dictionary<ItemPath, FuncDeclId> funcDeclsByPath;

            InternalGlobalVariableRepository internalGlobalVarRepo;

            public Context(                
                ItemInfoRepository itemInfoRepo,
                TypeValueService typeValueService,
                TypeExpInfoService typeExpTypeValueService,
                IErrorCollector errorCollector)
            {
                this.itemInfoRepo = itemInfoRepo;
                this.typeValueService = typeValueService;
                this.typeExpTypeValueService = typeExpTypeValueService;
                this.errorCollector = errorCollector;

                curFunc = new FuncContext(null, TypeValues.Int, false);
                bInLoop = false;
                internalGlobalVarRepo = new InternalGlobalVariableRepository();
                
                typeDecls = new List<TypeDecl>();
                funcDecls = new List<FuncDecl>();
                topLevelStmts = new List<Stmt>();
                funcDeclsByPath = new Dictionary<ItemPath, FuncDeclId>();
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return curFunc.DoesLocalVarNameExistInScope(name);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
            {
                errorCollector.Add(new AnalyzeError(code, node, msg));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
            {
                errorCollector.Add(new AnalyzeError(code, node, msg));
                throw new FatalAnalyzeException();
            }

            public ExternalGlobalVarId GetExternalGlobalVarId(ItemId varId)
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

                    if (!typeValueService.GetBaseTypeValue(curType, out var outType))
                        return false;

                    curType = outType;
                }

                return false;
            }

            public bool IsAssignableExp(Exp exp)
            {
                switch (exp)
                {
                    case LocalVarExp localVarExp:

                        // 람다 바깥에 있다면 대입 불가능하다
                        if (curFunc.IsLocalVarOutsideLambda(localVarExp.Name))
                            return false;

                        return true;
                    
                    case PrivateGlobalVarExp _:
                    case ListIndexerExp _:
                    case StaticMemberExp _:
                    case StructMemberExp _:
                    case ClassMemberExp _:
                    case EnumMemberExp _:
                        return true;

                    default:
                        return false;
                }

            }

            public void AddInternalGlobalVarInfo(Name name, TypeValue typeValue)
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
            
            public Type GetType(TypeValue typeValue)
            {
                bool Equals(TypeValue x, TypeValue y) => x.Equals(y);

                // 일단 predefined부터 걸러냅니다.
                if (typeValue is NormalTypeValue ntv)
                {
                    if (Equals(ntv, TypeValues.Bool)) return Type.Bool;
                    else if (Equals(ntv, TypeValues.Int)) return Type.Int;
                    else if (Equals(ntv, TypeValues.String)) return Type.String;
                    else if (ntv.GetTypeId().Equals(ItemIds.List))
                    {
                        var elemType = GetType(ntv.Entry.TypeArgs[0]);
                        return Type.List(elemType);
                    }                    
                }
                else if (typeValue is VoidTypeValue)
                    return Type.Void;

                throw new NotImplementedException();
            }

            public IEnumerable<LocalVarOutsideLambdaInfo> GetLocalVarsOutsideLambda()
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
                return typeExpTypeValueService.GetTypeExpInfo(typeExp);
            }

            LocalVarOutsideLambdaInfo? GetLocalVarOutsideLambdaInfo(string idName, ImmutableArray<TypeValue> typeArgs)
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length == 0)
                    return curFunc.GetLocalVarOutsideLambdaInfo(idName);

                return null;
            }

            // 지역 스코프에서 
            LocalVarInfo? GetLocalVarInfo(string idName, ImmutableArray<TypeValue> typeArgs)
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length == 0)
                    return curFunc.GetLocalVarInfo(idName);

                return null;
            }

            IdentifierInfo? GetThisMemberInfo(string idName, ImmutableArray<TypeValue> typeArgs)
            {
                // TODO: implementation
                return null;
            }

            InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName, ImmutableArray<TypeValue> typeArgs)
            {
                if (typeArgs.Length == 0)
                    return internalGlobalVarRepo.GetVariable(idName);

                return null;
            }

            TypeInfo? GetInternalGlobalTypeInfo(NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                var typeInfo = GlobalItemQueryService.GetGlobalItem(internalModuleInfo, namespacePath, new ItemPathEntry(idName)) as M.TypeInfo;
                if (typeInfo == null) return null;

                var typeValue = NormalTypeValue.MakeInternalGlobal(namespacePath, typeDeclId, typeInfo, typeArgs);
                var idInfo = new TypeInfo(typeValue);

                return idInfo;
            }

            IEnumerable<FuncInfo> GetInternalGlobalFuncInfos(NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                foreach(var mfuncInfo in GlobalItemQueryService.GetGlobalFuncs(internalModuleInfo, namespacePath, idName))
                {
                    // TODO: 인자 타입 추론을 사용하면, typeArgs를 생략할 수 있기 때문에, TypeArgs.Count가 TypeParams.Length보다 적어도 된다
                    if (typeArgs.Length == mfuncInfo.TypeParams.Length)
                    {
                        FuncDeclId funcDeclId;
                        var funcValue = new FuncValue(namespacePath, funcDeclId, mfuncInfo, typeArgs);
                        var funcInfo = new FuncInfo(funcValue);

                        yield return funcInfo;
                    }
                }
            }

            IEnumerable<TypeInfo> GetExternalGlobalTypeInfos(NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                throw new NotImplementedException();
            }

            IEnumerable<FuncInfo> GetExternalGlobalFuncInfos(NamespacePath namespacePath, string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                throw new NotImplementedException();
            }

            IdentifierInfo? GetGlobalInfo(string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                var candidates = new List<IdentifierInfo>();

                var curNamespacePath = NamespacePath.Root;

                var internalGlobalTypeInfo = GetInternalGlobalTypeInfo(curNamespacePath, idName, typeArgs, hintTypeValue);
                if (internalGlobalTypeInfo != null)
                    candidates.Add(internalGlobalTypeInfo);

                var internalGlobalFuncInfos = GetInternalGlobalFuncInfos(curNamespacePath, idName, typeArgs, hintTypeValue);
                candidates.AddRange(internalGlobalFuncInfos);

                var externalGlobalTypeInfos = GetExternalGlobalTypeInfos(curNamespacePath, idName, typeArgs, hintTypeValue);
                candidates.AddRange(externalGlobalTypeInfos);

                var externalGlobalFuncInfos = GetExternalGlobalFuncInfos(curNamespacePath, idName, typeArgs, hintTypeValue);
                candidates.AddRange(externalGlobalFuncInfos);

                // Global Identifier이므로 typeArgument의 최상위이다 (outer가 없다)
                foreach (var typeInfo in itemInfoRepo.GetTypes(NamespacePath.Root, new ItemPathEntry(idName, typeArgs.Length)))
                {
                    
                }

                // 힌트가 E고, First가 써져 있으면 E.First를 검색한다
                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hintTypeValue is NormalTypeValue hintNTV)
                {
                    // First<T> 같은건 없기 때문에 없을때만 검색한다
                    if (typeArgs.Length == 0)
                    {
                        var enumInfo = itemInfoRepo.GetItem<EnumInfo>(hintNTV.GetTypeId());
                        if (enumInfo != null)
                        {
                            if (enumInfo.GetElemInfo(idName, out var elemInfo))
                            {
                                var idInfo = new IdentifierInfo.EnumElem(hintNTV, elemInfo.Value);
                                candidates.Add(idInfo);
                            }
                        }
                    }
                }
                
                if (candidates.Count == 1)
                {
                    outIdInfo = candidates[0];
                    return true;
                }

                outIdInfo = null;
                return false;
            }

            public MemberVarValue? GetMemberVarValue(TypeValue typeValue, string memberName)
            {
                return typeValueService.GetMemberVarValue(typeValue, memberName);
            }

            public ItemPath? GetCurFuncPath()
            {
                return curFunc.GetFuncPath();
            }

            public void AddFuncDecl(ItemPath itemPath, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<string> paramNames, Stmt body)
            {
                var id = new FuncDeclId(funcDecls.Count);
                funcDecls.Add(new FuncDecl.Normal(id, bThisCall, typeParams, paramNames, body));
                funcDeclsByPath.Add(itemPath, id);
            }

            public void AddTopLevelStmt(Stmt stmt)
            {
                topLevelStmts.Add(stmt);
            }

            public IEnumerable<TypeDecl> GetTypeDecls()
            {
                return typeDecls;
            }

            public IEnumerable<FuncDecl> GetFuncDecls()
            {
                return funcDecls;
            }

            public IEnumerable<Stmt> GetTopLevelStmts()
            {
                return topLevelStmts;
            }

            public void AddSeqFuncDecl(ItemPath itemPath, Type retTypeId, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<string> paramNames, Stmt body)
            {
                var id = new FuncDeclId(funcDecls.Count);
                funcDecls.Add(new FuncDecl.Sequence(id, retTypeId, bThisCall, typeParams, paramNames,body));
                funcDeclsByPath.Add(itemPath, id);
            }

            public IdentifierInfo? GetIdentifierInfo(string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                // 0. 람다 바깥의 local 변수
                var localOutsideInfo = GetLocalVarOutsideLambdaInfo(idName, typeArgs);
                if (localOutsideInfo != null) return localOutsideInfo;

                // 1. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                var localVarInfo = GetLocalVarInfo(idName, typeArgs);
                if (localVarInfo != null) return localVarInfo;

                // 2. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                var thisMemberInfo = GetThisMemberInfo(idName, typeArgs);
                if (thisMemberInfo != null) return thisMemberInfo;

                // 3. internal global 'variable', 변수이므로 힌트를 쓸 일이 없다
                var internalGlobalVarInfo = GetInternalGlobalVarInfo(idName, typeArgs);
                if (internalGlobalVarInfo != null) return internalGlobalVarInfo;

                // 4. 네임스페이스 -> 바깥 네임스페이스 -> module global, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                var externalGlobalInfo = GetGlobalInfo(idName, typeArgs, hintTypeValue);
                if (externalGlobalInfo != null) return externalGlobalInfo;

                return null;
            }

            public FuncTypeValue GetTypeValue(FuncValue funcValue)
            {
                return typeValueService.GetTypeValue(funcValue);
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

            public FuncDeclId? GetFuncDeclId(ItemPath funcPath)
            {
                if (funcDeclsByPath.TryGetValue(funcPath, out var funcDeclId))
                    return funcDeclId;
                else
                    return null;
            }

            public IEnumerable<FuncInfo> GetFuncs(NamespacePath root, string value)
            {
                return itemInfoRepo.GetFuncs(root, value);
            }

            public TypeValue Apply(TypeValue context, TypeValue typeValue)
            {
                return typeValueService.Apply(context, typeValue);
            }

            // 1. exp가 무슨 타입을 가지는지
            // 2. callExp가 staticFunc을 호출할 경우 무슨 함수를 호출하는지
        }
    }
}
