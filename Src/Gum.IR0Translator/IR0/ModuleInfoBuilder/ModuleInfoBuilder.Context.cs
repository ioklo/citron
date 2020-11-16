using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class ModuleInfoBuilder
    {
        class Context
        {
            private SyntaxNodeModuleItemService syntaxNodeModuleItemService;
            private TypeExpTypeValueService typeExpTypeValueService;

            private TypeBuilder? typeBuilder;
            private List<TypeInfo> globalTypeInfos; // global 타입 정보
            private List<FuncInfo> globalFuncInfos; // All Funcs
            private List<VarInfo> globalVarInfos;   // global Variable정보
            private Dictionary<S.FuncDecl, FuncInfo> funcInfosByDecl;
            private Dictionary<S.TypeDecl, TypeInfo> typeInfosByDecl;

            public Context(
                SyntaxNodeModuleItemService syntaxNodeModuleItemService,
                TypeExpTypeValueService typeExpTypeValueService)
            {
                this.syntaxNodeModuleItemService = syntaxNodeModuleItemService;
                this.typeExpTypeValueService = typeExpTypeValueService;

                typeBuilder = null;
                globalTypeInfos = new List<TypeInfo>();
                globalFuncInfos = new List<FuncInfo>();
                globalVarInfos = new List<VarInfo>();
                funcInfosByDecl = new Dictionary<S.FuncDecl, FuncInfo>();
                typeInfosByDecl = new Dictionary<S.TypeDecl, TypeInfo>();
            }

            public ItemPath GetTypePath(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetTypePath(node);
            }

            public TypeValue GetTypeValue(S.TypeExp typeExp)
            {
                return typeExpTypeValueService.GetTypeValue(typeExp);
            }

            public ItemPath GetFuncPath(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetFuncPath(node);
            }

            public ItemPath? GetThisTypePath()
            {
                if (typeBuilder == null)
                    return null;

                return typeBuilder.GetThisTypeAppliedPath().GetItemPath();
            }

            public void AddTypeInfo(S.TypeDecl typeDecl, TypeInfo typeInfo)
            {
                if (typeBuilder == null)
                    globalTypeInfos.Add(typeInfo);
                else
                    typeBuilder.AddTypeInfo(typeInfo);

                typeInfosByDecl[typeDecl] = typeInfo;
            }

            public void AddFuncInfo(S.FuncDecl funcDecl, FuncInfo funcInfo)
            {
                if (typeBuilder == null)
                    globalFuncInfos.Add(funcInfo);
                else
                    typeBuilder.AddFuncInfo(funcInfo);

                funcInfosByDecl[funcDecl] = funcInfo;
            }

            public void AddVarInfo(VarInfo varInfo)
            {
                if (typeBuilder == null)
                    globalVarInfos.Add(varInfo);
                else
                    typeBuilder.AddVarInfo(varInfo);
            }

            public ImmutableDictionary<S.FuncDecl, FuncInfo> GetFuncsByFuncDecl()
            {
                return funcInfosByDecl.ToImmutableDictionary();
            }

            public ImmutableDictionary<S.TypeDecl, TypeInfo> GetTypeInfosByDecl()
            {
                return typeInfosByDecl.ToImmutableDictionary();
            }

            public IEnumerable<ItemInfo> GetGlobalItems()
            {
                return globalTypeInfos.Cast<ItemInfo>()
                    .Concat(globalFuncInfos)
                    .Concat(globalVarInfos);
            }
        }
    }
}