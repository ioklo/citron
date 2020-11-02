using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
            private List<ITypeInfo> typeInfos; // All Types
            private List<FuncInfo> funcInfos; // All Funcs
            private List<VarInfo> varInfos; // Type의 Variable
            private Dictionary<S.FuncDecl, FuncInfo> funcInfosByDecl;
            private Dictionary<S.EnumDecl, EnumInfo> enumInfosByDecl;

            public Context(
                SyntaxNodeModuleItemService syntaxNodeModuleItemService,
                TypeExpTypeValueService typeExpTypeValueService)
            {
                this.syntaxNodeModuleItemService = syntaxNodeModuleItemService;
                this.typeExpTypeValueService = typeExpTypeValueService;

                typeBuilder = null;
                typeInfos = new List<ITypeInfo>();
                funcInfos = new List<FuncInfo>();
                varInfos = new List<VarInfo>();
                funcInfosByDecl = new Dictionary<S.FuncDecl, FuncInfo>();
                enumInfosByDecl = new Dictionary<S.EnumDecl, EnumInfo>();
            }

            public ModuleItemId GetTypeId(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetTypeId(node);
            }

            public TypeValue GetTypeValue(S.TypeExp typeExp)
            {
                return typeExpTypeValueService.GetTypeValue(typeExp);
            }

            public ModuleItemId GetFuncId(S.ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetFuncId(node);
            }

            public TypeValue.Normal? GetThisTypeValue()
            {
                if (typeBuilder == null)
                    return null;

                return typeBuilder.GetThisTypeValue();
            }

            public void AddEnumInfo(S.EnumDecl enumDecl, EnumInfo enumInfo)
            {
                typeInfos.Add(enumInfo);
                enumInfosByDecl[enumDecl] = enumInfo;
            }

            public void AddFuncInfo(S.FuncDecl? funcDecl, FuncInfo funcInfo)
            {
                funcInfos.Add(funcInfo);
                if (funcDecl != null)
                    funcInfosByDecl[funcDecl] = funcInfo;
            }

            public void AddVarInfo(VarInfo varInfo)
            {
                varInfos.Add(varInfo);
            }

            public ImmutableArray<ITypeInfo> GetTypeInfos()
            {
                return typeInfos.ToImmutableArray();
            }

            public ImmutableArray<FuncInfo> GetFuncInfos()
            {
                return funcInfos.ToImmutableArray();
            }

            public ImmutableArray<VarInfo> GetVarInfos()
            {
                return varInfos.ToImmutableArray();
            }

            public ImmutableDictionary<S.FuncDecl, FuncInfo> GetFuncsByFuncDecl()
            {
                return funcInfosByDecl.ToImmutableDictionary();
            }

            public ImmutableDictionary<S.EnumDecl, EnumInfo> GetEnumInfosByDecl()
            {
                return enumInfosByDecl.ToImmutableDictionary();
            }

            
        }
    }
}