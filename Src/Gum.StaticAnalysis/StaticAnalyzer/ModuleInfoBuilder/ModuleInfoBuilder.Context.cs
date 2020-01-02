using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.StaticAnalysis
{
    public partial class ModuleInfoBuilder
    {
        class Context
        {
            private SyntaxNodeModuleItemService syntaxNodeModuleItemService;
            private TypeExpTypeValueService typeExpTypeValueService;

            private TypeBuilder? typeBuilder;
            private List<ITypeInfo> typeInfos; // All Types
            private List<FuncInfo> funcInfos; // All Funcs
            private List<VarInfo> varInfos; // Type의 Variable
            private Dictionary<FuncDecl, FuncInfo> funcInfosByDecl;
            private Dictionary<EnumDecl, EnumInfo> enumInfosByDecl;

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
                funcInfosByDecl = new Dictionary<FuncDecl, FuncInfo>(RefEqComparer<FuncDecl>.Instance);
                enumInfosByDecl = new Dictionary<EnumDecl, EnumInfo>(RefEqComparer<EnumDecl>.Instance);
            }

            public ModuleItemId GetTypeId(ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetTypeId(node);
            }

            public TypeValue GetTypeValue(TypeExp typeExp)
            {
                return typeExpTypeValueService.GetTypeValue(typeExp);
            }

            public ModuleItemId GetFuncId(ISyntaxNode node)
            {
                return syntaxNodeModuleItemService.GetFuncId(node);
            }

            public TypeValue.Normal? GetThisTypeValue()
            {
                if (typeBuilder == null)
                    return null;

                return typeBuilder.GetThisTypeValue();
            }

            public void AddEnumInfo(EnumDecl enumDecl, EnumInfo enumInfo)
            {
                typeInfos.Add(enumInfo);
                enumInfosByDecl[enumDecl] = enumInfo;
            }

            public void AddFuncInfo(FuncDecl? funcDecl, FuncInfo funcInfo)
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

            public ImmutableDictionary<FuncDecl, FuncInfo> GetFuncsByFuncDecl()
            {
                return funcInfosByDecl.ToImmutableWithComparer();
            }

            public ImmutableDictionary<EnumDecl, EnumInfo> GetEnumInfosByDecl()
            {
                return enumInfosByDecl.ToImmutableWithComparer();
            }

            
        }
    }
}