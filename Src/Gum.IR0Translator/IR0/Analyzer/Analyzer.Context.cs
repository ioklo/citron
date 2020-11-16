using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 
        public class Context
        {
            public struct PrivateGlobalVarInfo
            {
                public string Name { get; }
                public TypeValue TypeValue { get; }
                public PrivateGlobalVarInfo(string name, TypeValue typeValue)
                {
                    Name = name;
                    TypeValue = typeValue;
                }
            }

            public TItemInfo? GetItem<TItemInfo>(ItemId id)
                where TItemInfo : ItemInfo
            {
                return itemInfoRepo.GetItem<TItemInfo>(id);
            }
            
            private ItemInfoRepository itemInfoRepo;

            public TypeValueService TypeValueService { get; }

            IErrorCollector errorCollector;

            // 현재 실행되고 있는 함수
            private FuncContext curFunc;

            // CurFunc와 bGlobalScope를 나누는 이유는, globalScope에서 BlockStmt 안으로 들어가면 global이 아니기 때문이다
            private bool bGlobalScope;
            private bool bInLoop;
            private ImmutableDictionary<S.FuncDecl, FuncInfo> funcInfosByDecl;
            private ImmutableDictionary<S.TypeDecl, TypeInfo> typeInfosByDecl;
            private TypeExpTypeValueService typeExpTypeValueService;
            private Dictionary<string, PrivateGlobalVarInfo> privateGlobalVarInfos;
            private List<TypeDecl> typeDecls;
            private List<FuncDecl> funcDecls;
            private Dictionary<ItemId, FuncDeclId> funcDeclsById;

            public Context(                
                ItemInfoRepository itemInfoRepo,
                TypeValueService typeValueService,
                TypeExpTypeValueService typeExpTypeValueService,
                ImmutableDictionary<S.FuncDecl, FuncInfo> funcInfosByDecl,
                ImmutableDictionary<S.TypeDecl, TypeInfo> typeInfosByDecl,
                IErrorCollector errorCollector)
            {
                this.itemInfoRepo = itemInfoRepo;

                TypeValueService = typeValueService;

                this.funcInfosByDecl = funcInfosByDecl;
                this.typeInfosByDecl = typeInfosByDecl;
                this.typeExpTypeValueService = typeExpTypeValueService;

                this.errorCollector = errorCollector;

                curFunc = new FuncContext(new TypeValue.Normal(ItemIds.Int), false);
                bGlobalScope = true;
                bInLoop = false;
                privateGlobalVarInfos = new Dictionary<string, PrivateGlobalVarInfo>();
                
                typeDecls = new List<TypeDecl>();
                funcDecls = new List<FuncDecl>();
                funcDeclsById = new Dictionary<ItemId, FuncDeclId>();
            }

            public bool DoesLocalVarNameExistInScope(string name)
            {
                return curFunc.DoesLocalVarNameExistInScope(name);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
            {
                errorCollector.Add(new AnalyzeError(code, node, msg));
            }

            public ExternalGlobalVarId GetExternalGlobalVarId(ItemId varId)
            {
                throw new NotImplementedException();
            }

            public void AddPrivateGlobalVarInfo(string name, TypeValue typeValue)
            {
                privateGlobalVarInfos.Add(name, new PrivateGlobalVarInfo(name, typeValue));
            }

            public bool GetPrivateGlobalVarInfo(string value, out PrivateGlobalVarInfo privateGlobalVarInfo)
            {
                return privateGlobalVarInfos.TryGetValue(value, out privateGlobalVarInfo);
            }

            public int GetPrivateGlobalVarCount()
            {
                return privateGlobalVarInfos.Count;
            }

            public void ExecInLocalScope(Action action)
            {
                var bPrevGlobalScope = bGlobalScope;
                bGlobalScope = false;

                curFunc.ExecInLocalScope(action);

                bGlobalScope = bPrevGlobalScope;
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
                // 일단 predefined부터 걸러냅니다.
                if (typeValue is TypeValue.Normal ntv)
                {
                    if (ntv == TypeValues.Bool) return Type.Bool;
                    else if (ntv == TypeValues.Int) return Type.Int;
                    else if (ntv == TypeValues.String) return Type.String;
                    else if (ModuleInfoEqualityComparer.EqualsItemId(ntv.GetTypeId(), ItemIds.List))
                    {
                        var elemType = GetType(ntv.Entry.TypeArgs[0]);
                        return Type.List(elemType);
                    }                    
                }
                else if (typeValue is TypeValue.Void)
                    return Type.Void;

                throw new NotImplementedException();
            }

            public IEnumerable<LocalVarOutsideLambdaInfo> GetLocalVarsOutsideLambda()
            {
                return curFunc.GetLocalVarsOutsideLambda();
            }

            public (bool IsSuccess, TypeValue? RetTypeValue) ExecInLambdaScope(TypeValue? lambdaRetTypeValue, Func<bool> action)
            {
                return curFunc.ExecInLambdaScope(lambdaRetTypeValue, action);
            }

            internal bool IsLocalVarOutsideLambda(string name)
            {
                return curFunc.IsLocalVarOutsideLambda(name);
            }

            public void ExecInFuncScope(FuncContext funcContext, Action action)
            {
                var (prevFunc, bPrevGlobalScope) = (curFunc, bGlobalScope);
                bGlobalScope = false;
                curFunc = funcContext;

                try
                {
                    action.Invoke();
                }
                finally
                {
                    bGlobalScope = bPrevGlobalScope;
                    curFunc = prevFunc;
                }
            }


            public bool GetTypeValueByName(string varName, [NotNullWhen(true)] out TypeValue? localVarTypeValue)
            {
                throw new NotImplementedException();
            }

            public FuncInfo GetFuncInfoByDecl(S.FuncDecl funcDecl)
            {
                return funcInfosByDecl[funcDecl];
            }

            public TTypeInfo GetTypeInfoByDecl<TTypeInfo>(S.EnumDecl enumDecl)
                where TTypeInfo : TypeInfo
            {
                return (TTypeInfo)typeInfosByDecl[enumDecl];
            }

            public bool IsGlobalScope()
            {
                return bGlobalScope;
            }

            public bool IsInLoop()
            {
                return bInLoop;
            }

            public void SetGlobalScope(bool bNewGlobalScope)
            {
                bGlobalScope = bNewGlobalScope;
            }

            public TypeValue GetTypeValueByTypeExp(S.TypeExp typeExp)
            {
                return typeExpTypeValueService.GetTypeValue(typeExp);
            }

            private bool GetLocalOutsideLambdaIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                [NotNullWhen(true)] out IdentifierInfo? outIdInfo)
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Count == 0)
                    if (curFunc.GetLocalVarOutsideLambdaInfo(idName, out var localVarOutsideInfo))
                    {
                        outIdInfo = new IdentifierInfo.LocalOutsideLambda(localVarOutsideInfo);                        
                        return true;
                    }

                outIdInfo = null;
                return false;
            }

            // 지역 스코프에서 
            private bool GetLocalIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                [NotNullWhen(true)] out IdentifierInfo? outIdInfo)
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Count == 0)
                    if (curFunc.GetLocalVarInfo(idName, out var localVarInfo))
                    {
                        outIdInfo = new IdentifierInfo.Local(localVarInfo.Name, localVarInfo.TypeValue);
                        return true;
                    }

                outIdInfo = null;
                return false;
            }

            private bool GetThisIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                [NotNullWhen(true)] out IdentifierInfo? idInfo)
            {
                // TODO: implementation

                idInfo = null;
                return false;
            }

            private bool GetPrivateGlobalVarIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                [NotNullWhen(true)] out IdentifierInfo? outIdInfo)
            {
                if (typeArgs.Count == 0)
                    if (privateGlobalVarInfos.TryGetValue(idName, out var privateGlobalVarInfo))
                    {
                        outIdInfo = new IdentifierInfo.PrivateGlobal(privateGlobalVarInfo.Name, privateGlobalVarInfo.TypeValue);
                        return true;
                    }

                outIdInfo = null;
                return false;
            }

            private bool GetModuleGlobalIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                TypeValue? hintTypeValue,
                [NotNullWhen(true)] out IdentifierInfo? outIdInfo)
            {
                var candidates = new List<IdentifierInfo>();

                // TODO: root 이외의 namespace 지원                
                if (typeArgs.Count == 0)
                {
                    foreach (var varInfo in itemInfoRepo.GetVars(NamespacePath.Root, idName))
                    {
                        var varId = varInfo.GetId();
                        Debug.Assert(varId.OuterEntries.Length == 0);

                        var idInfo = new IdentifierInfo.ModuleGlobal(
                            new VarValue(varId.ModuleName, varId.NamespacePath, varId.Entry.Name), varInfo.TypeValue);
                        candidates.Add(idInfo);
                    }
                }

                // Global Identifier이므로 typeArgument의 최상위이다 (outer가 없다)
                foreach (var funcInfo in itemInfoRepo.GetFuncs(NamespacePath.Root, idName))
                {
                    // TODO: 인자 타입 추론을 사용하면, typeArgs를 생략할 수 있기 때문에, TypeArgs.Count가 TypeParams.Length보다 적어도 된다
                    if (typeArgs.Count == funcInfo.TypeParams.Length)
                    {
                        var funcId = funcInfo.GetId();
                        Debug.Assert(funcId.OuterEntries.Length == 0);

                        var idInfo = new IdentifierInfo.Func(
                            new FuncValue(funcId.ModuleName, funcId.NamespacePath, new AppliedItemPathEntry(funcId.Entry.Name, funcId.Entry.ParamHash, typeArgs))
                        );

                        candidates.Add(idInfo);
                    }
                }

                foreach (var typeInfo in itemInfoRepo.GetTypes(NamespacePath.Root, new ItemPathEntry(idName, typeArgs.Count)))
                {
                    var typeId = typeInfo.GetId();

                    var idInfo = new IdentifierInfo.Type(new TypeValue.Normal(
                        typeId.ModuleName,
                        typeId.NamespacePath,
                        new AppliedItemPathEntry(typeId.Entry.Name, typeId.Entry.ParamHash, typeArgs)                        
                    ));

                    candidates.Add(idInfo);
                }

                // 힌트가 E고, First가 써져 있으면 E.First를 검색한다
                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hintTypeValue is TypeValue.Normal hintNTV)
                {
                    // First<T> 같은건 없기 때문에 없을때만 검색한다
                    if (typeArgs.Count == 0)
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

            public void AddFuncDecl(ItemId itemId, bool bThisCall, IEnumerable<string> typeParams, IEnumerable<string> paramNames, Stmt body)
            {
                var id = new FuncDeclId(funcDecls.Count);
                funcDecls.Add(new FuncDecl.Normal(id, bThisCall, typeParams, paramNames, body));
                funcDeclsById.Add(itemId, id);
            }

            public IEnumerable<TypeDecl> GetTypeDecls()
            {
                return typeDecls;
            }

            public IEnumerable<FuncDecl> GetFuncDecls()
            {
                return funcDecls;
            }

            public void AddSeqFunc(ItemId itemId, Type retTypeId, bool bThisCall, IEnumerable<string> typeParams, IEnumerable<string> paramNames, Stmt body)
            {
                var id = new FuncDeclId(funcDecls.Count);
                funcDecls.Add(new FuncDecl.Sequence(id, retTypeId, bThisCall, typeParams, paramNames,body));
                funcDeclsById.Add(itemId, id);
            }

            public bool GetIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                TypeValue? hintTypeValue,
                [NotNullWhen(true)] out IdentifierInfo? outIdInfo)
            {
                // 0. 람다 바깥의 local 변수
                if (GetLocalOutsideLambdaIdentifierInfo(idName, typeArgs, out outIdInfo))
                    return true;

                // 1. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                if (GetLocalIdentifierInfo(idName, typeArgs, out outIdInfo))
                    return true;

                // 2. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                if (GetThisIdentifierInfo(idName, typeArgs, out outIdInfo))
                    return true;

                // 3. private global 'variable', 변수이므로 힌트를 쓸 일이 없다
                if (GetPrivateGlobalVarIdentifierInfo(idName, typeArgs, out outIdInfo))
                    return true;

                // 4. module global, 변수, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                if (GetModuleGlobalIdentifierInfo(idName, typeArgs, hintTypeValue, out outIdInfo))
                    return true;

                outIdInfo = null;
                return false;
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

            public bool DoesPrivateGlobalVarNameExist(string name)
            {
                return privateGlobalVarInfos.ContainsKey(name);
            }

            public FuncDeclId? GetFuncDeclId(ItemId funcId)
            {
                if (funcDeclsById.TryGetValue(funcId, out var funcDeclId))
                    return funcDeclId;
                else
                    return null;
            }

            public IEnumerable<FuncInfo> GetFuncs(NamespacePath root, string value)
            {
                return itemInfoRepo.GetFuncs(root, value);
            }

            // 1. exp가 무슨 타입을 가지는지
            // 2. callExp가 staticFunc을 호출할 경우 무슨 함수를 호출하는지
        }
    }
}
