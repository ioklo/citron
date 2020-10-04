using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
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

            public ModuleInfoService ModuleInfoService { get; }

            public TypeValueService TypeValueService { get; }

            public IErrorCollector ErrorCollector { get; }

            // 현재 실행되고 있는 함수
            private FuncContext curFunc;

            // CurFunc와 bGlobalScope를 나누는 이유는, globalScope에서 BlockStmt 안으로 들어가면 global이 아니기 때문이다
            private bool bGlobalScope;
            private ImmutableDictionary<FuncDecl, FuncInfo> funcInfosByDecl;
            private ImmutableDictionary<EnumDecl, EnumInfo> enumInfosByDecl;
            private TypeExpTypeValueService typeExpTypeValueService;
            private Dictionary<string, PrivateGlobalVarInfo> privateGlobalVarInfos;            
            private List<ScriptTemplate> templates;

            public Context(
                ModuleInfoService moduleInfoService,
                TypeValueService typeValueService,
                TypeExpTypeValueService typeExpTypeValueService,
                ImmutableDictionary<FuncDecl, FuncInfo> funcInfosByDecl,
                ImmutableDictionary<EnumDecl, EnumInfo> enumInfosByDecl,
                IErrorCollector errorCollector)
            {
                ModuleInfoService = moduleInfoService;
                TypeValueService = typeValueService;

                this.funcInfosByDecl = funcInfosByDecl;
                this.enumInfosByDecl = enumInfosByDecl;
                this.typeExpTypeValueService = typeExpTypeValueService;

                ErrorCollector = errorCollector;

                curFunc = new FuncContext(null, null, false);
                bGlobalScope = true;
                privateGlobalVarInfos = new Dictionary<string, PrivateGlobalVarInfo>();
                
                templates = new List<ScriptTemplate>();
            }            

            public void AddOverrideVarInfo(StorageInfo storageInfo, TypeValue testTypeValue)
            {
                curFunc.AddOverrideVarInfo(storageInfo, testTypeValue);
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

            public FuncInfo GetFuncInfoByDecl(FuncDecl funcDecl)
            {
                return funcInfosByDecl[funcDecl];
            }

            public EnumInfo GetEnumInfoByDecl(EnumDecl enumDecl)
            {
                return enumInfosByDecl[enumDecl];
            }

            public bool IsGlobalScope()
            {
                return bGlobalScope;
            }

            public void SetGlobalScope(bool bNewGlobalScope)
            {
                bGlobalScope = bNewGlobalScope;
            }

            public TypeValue GetTypeValueByTypeExp(TypeExp typeExp)
            {
                return typeExpTypeValueService.GetTypeValue(typeExp);
            }
            
            public void AddTemplate(ScriptTemplate funcTempl)
            {
                templates.Add(funcTempl);
            }

            private IdentifierInfo MakeVarIdentifierInfo(StorageInfo storageInfo, TypeValue typeValue)
            {
                if (curFunc.ShouldOverrideTypeValue(storageInfo, typeValue, out var overriddenTypeValue))
                    return IdentifierInfo.MakeVar(storageInfo, overriddenTypeValue);

                return IdentifierInfo.MakeVar(storageInfo, typeValue);
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
                        var storageInfo = StorageInfo.MakeLocal(localVarInfo.Index);
                        var typeValue = localVarInfo.TypeValue;

                        outIdInfo = MakeVarIdentifierInfo(storageInfo, typeValue);
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
                        var storageInfo = StorageInfo.MakePrivateGlobal(privateGlobalVarInfo.Index);
                        var typeValue = privateGlobalVarInfo.TypeValue;

                        outIdInfo = MakeVarIdentifierInfo(storageInfo, typeValue);
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
                var itemId = ModuleItemId.Make(idName, typeArgs.Count);

                var candidates = new List<IdentifierInfo>();

                // id에 typeCount가 들어가므로 typeArgs.Count검사는 하지 않는다
                foreach (var varInfo in ModuleInfoService.GetVarInfos(itemId))
                {
                    var storageInfo = StorageInfo.MakeModuleGlobal(itemId);
                    var typeValue = varInfo.TypeValue;

                    var idInfo = MakeVarIdentifierInfo(storageInfo, typeValue);
                    candidates.Add(idInfo);
                }

                // Global Identifier이므로 typeArgument의 최상위이다 (outer가 없다)
                var typeArgList = TypeArgumentList.Make(null, typeArgs);

                foreach (var funcInfo in ModuleInfoService.GetFuncInfos(itemId))
                {
                    var idInfo = IdentifierInfo.MakeFunc(new FuncValue(funcInfo.FuncId, typeArgList));
                    candidates.Add(idInfo);
                }

                foreach (var typeInfo in ModuleInfoService.GetTypeInfos(itemId))
                {
                    var idInfo = IdentifierInfo.MakeType(TypeValue.MakeNormal(typeInfo.TypeId, typeArgList));
                    candidates.Add(idInfo);
                }

                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hintTypeValue is TypeValue.Normal hintNTV && typeArgs.Count == 0)
                {
                    // hintNTV가 최상위 타입이라는 것을 확인하기 위해 TypeArgList의 Outer를 확인했다.
                    if (hintNTV.TypeArgList.Outer == null)
                    {
                        var hintTypeInfo = ModuleInfoService.GetTypeInfos(hintNTV.TypeId).Single();
                        if( hintTypeInfo is IEnumInfo enumTypeInfo)
                        {
                            if (enumTypeInfo.GetElemInfo(idName, out var elemInfo))
                            {
                                var idInfo = IdentifierInfo.MakeEnumElem(hintNTV, elemInfo.Value);
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

            public bool GetIdentifierInfo(
                string idName, IReadOnlyList<TypeValue> typeArgs,
                TypeValue? hintTypeValue,
                [NotNullWhen(true)] out IdentifierInfo? idInfo)
            {
                // 1. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                if (GetLocalIdentifierInfo(idName, typeArgs, out idInfo))
                    return true;

                // 2. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                if (GetThisIdentifierInfo(idName, typeArgs, out idInfo))
                    return true;

                // 3. private global 'variable', 변수이므로 힌트를 쓸 일이 없다
                if (GetPrivateGlobalVarIdentifierInfo(idName, typeArgs, out idInfo))
                    return true;

                // 4. module global, 변수, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                if (GetModuleGlobalIdentifierInfo(idName, typeArgs, hintTypeValue, out idInfo))
                    return true;

                idInfo = null;
                return false;
            }


            internal ModuleItemId MakeLabmdaFuncId()
            {
                return curFunc.MakeLambdaFuncId();
            }

            internal IEnumerable<ScriptTemplate> GetTemplates()
            {
                return templates;
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

            internal void SetRetTypeValue(TypeValue retTypeValue)
            {
                curFunc.SetRetTypeValue(retTypeValue);
            }

            // 1. exp가 무슨 타입을 가지는지
            // 2. callExp가 staticFunc을 호출할 경우 무슨 함수를 호출하는지
        }
    }
}
