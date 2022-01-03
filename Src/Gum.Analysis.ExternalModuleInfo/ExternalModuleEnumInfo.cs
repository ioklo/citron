using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleEnumInfo : IModuleEnumDecl
    {
        IModuleItemDecl outer;
        M.ItemPathEntry entry;
        ImmutableDictionary<M.Name, ExternalModuleEnumElemInfo> elemInfos;

        public ExternalModuleEnumInfo(IModuleItemDecl outer, M.EnumInfo enumInfo)
        {
            this.outer = outer;

            entry = new M.ItemPathEntry(enumInfo.Name, enumInfo.TypeParams.Length);

            var builder = ImmutableDictionary.CreateBuilder<M.Name, ExternalModuleEnumElemInfo>();
            foreach (var elemInfo in enumInfo.ElemInfos)
                builder.Add(elemInfo.Name, new ExternalModuleEnumElemInfo(this, elemInfo));

            elemInfos = builder.ToImmutable();
        }

        IModuleItemDecl? IModuleItemDecl.GetOuter()
        {
            return outer;
        }

        IModuleNamespaceInfo? IModuleItemDecl.GetNamespace(M.Name name)
        {
            return null;
        }

        M.ItemPathEntry IModuleItemDecl.GetEntry()
        {
            return entry;
        }

        IModuleEnumElemDecl? IModuleEnumDecl.GetElem(M.Name memberName)
        {
            return elemInfos.GetValueOrDefault(memberName);
        }

        IModuleFuncDecl? IModuleItemDecl.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        ImmutableArray<IModuleFuncDecl> IModuleItemDecl.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return default;
        }

        IModuleTypeDecl? IModuleItemDecl.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }
    }
}
