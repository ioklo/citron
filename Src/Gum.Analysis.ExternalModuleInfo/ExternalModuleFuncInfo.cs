using Gum.Collections;
using Gum.Infra;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class ExternalModuleFuncInfo : IModuleFuncDecl
    {
        IModuleItemDecl? outer;
        M.FuncInfo funcInfo;

        public ExternalModuleFuncInfo(IModuleItemDecl? outer, M.FuncInfo funcInfo)
        {
            this.outer = outer;
            this.funcInfo = funcInfo;
        }

        IModuleItemDecl? GetOuter()
        {
            return outer;
        }


        M.AccessModifier IModuleCallableDecl.GetAccessModifier()
        {
            return funcInfo.AccessModifier;
        }

        M.ItemPathEntry GetEntry()
        {
            return new M.ItemPathEntry(funcInfo.Name, funcInfo.TypeParams.Length);
        }

        ImmutableArray<M.Param> IModuleCallableDecl.GetParameters()
        {
            return funcInfo.Parameters;
        }
        
        M.Type IModuleFuncDecl.GetReturnType()
        {
            return funcInfo.RetType;
        }        

        bool IModuleFuncDecl.IsInstanceFunc()
        {
            return funcInfo.IsInstanceFunc;
        }

        bool IModuleFuncDecl.IsInternal()
        {
            return false;
        }

        bool IModuleFuncDecl.IsSequenceFunc()
        {
            return funcInfo.IsSequenceFunc;
        }        
    }
}
