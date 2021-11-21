using Gum.Infra;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // ModuleInfo를 검색할때 쓰는 인터페이스, internal, external둘다 사용한다
    // M.ModuleInfo 대체
    public interface IModuleInfo : IModuleNamespaceContainer, IModuleTypeContainer, IModuleFuncContainer, IPure
    {
        M.ModuleName GetName();
        IModuleItemInfo? GetItem(M.Name name, int typeParamCount, M.ParamTypes paramTypes);
    }
}