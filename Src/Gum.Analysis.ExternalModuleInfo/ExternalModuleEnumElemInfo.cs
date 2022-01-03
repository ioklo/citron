using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class ExternalModuleEnumElemInfo : IModuleEnumElemDecl
    {
        IModuleEnumDecl outer;
        M.EnumElemInfo elemInfo;
        ImmutableArray<IModuleMemberVarInfo> memberVarInfos;

        public ExternalModuleEnumElemInfo(ExternalModuleEnumInfo outer, M.EnumElemInfo elemInfo)
        {
            this.outer = outer;
            this.elemInfo = elemInfo;

            var builder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elemInfo.FieldInfos.Length);

            foreach (var fieldInfo in elemInfo.FieldInfos)
                builder.Add(new ExternalModuleMemberVarInfo(fieldInfo));

            memberVarInfos = builder.MoveToImmutable();
        }
        
        ImmutableArray<IModuleMemberVarInfo> IModuleEnumElemDecl.GetFieldInfos()
        {
            return memberVarInfos;
        }

        IModuleItemDecl? IModuleItemDecl.GetOuter()
        {
            return outer;
        }

        IModuleNamespaceInfo? IModuleItemDecl.GetNamespace(M.Name name)
        {
            return null;
        }

        IModuleFuncDecl? IModuleItemDecl.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncDecl> IModuleItemDecl.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        M.ItemPathEntry IModuleItemDecl.GetEntry()
        {
            return new M.ItemPathEntry(elemInfo.Name, elemInfo.TypeParams.Length);
        }

        IModuleTypeDecl? IModuleItemDecl.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }        

        bool IModuleEnumElemDecl.IsStandalone()
        {
            return elemInfo.FieldInfos.Length == 0;
        }
    }
}
