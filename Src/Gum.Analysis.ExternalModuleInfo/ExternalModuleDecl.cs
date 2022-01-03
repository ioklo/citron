using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record ExternalModuleGlobalTypeDecl(ExternalModuleTypeDecl TypeDecl) : IModuleGlobalTypeDecl;

    [ImplementIEquatable]
    public partial class ExternalModuleDecl : IModuleDecl
    {
        M.Name name;
        ImmutableDictionary<M.RootTypeDeclPath, ExternalModuleGlobalTypeDecl> globalTypes;

        public M.Name GetName()
        {
            return name;
        }

        public ExternalModuleDecl(M.ModuleDecl moduleDecl)
        {
            name = moduleDecl.Name;

            // globalTypes
            var builder = ImmutableDictionary.CreateBuilder<M.RootTypeDeclPath, ExternalModuleGlobalTypeDecl>();
            foreach (var type in moduleDecl.Types)
            {
                var typeDecl = ExternalModuleMisc.Make(type.TypeDecl);
                var globalTypeDecl = new ExternalModuleGlobalTypeDecl(typeDecl);

                builder.Add(new M.RootTypeDeclPath(), globalTypeDecl);
            }
            globalTypes = builder.ToImmutable();


            typeDict = ExternalModuleMisc.MakeModuleTypeDict(moduleInfo.Types);            
            funcDict = ExternalModuleMisc.MakeModuleFuncDict(this, moduleInfo.Funcs);
        }

        IModuleItemDecl? GetOuter()
        {
            return null;
        }

        M.ItemPathEntry GetEntry()
        {
            return new M.ItemPathEntry(name);
        }

        public IModuleTypeDecl? GetGlobalType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<IModuleFuncDecl> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        IModuleFuncDecl? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }

        // 직계        
        public IModuleTypeDecl? GetType(RootTypeDeclPath path)
        {

        }

        public IModuleFuncDecl? GetFunc(RootFuncDeclPath path)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<IModuleFuncDecl> GetFuncs(NamespacePath path, Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }
    }
}
