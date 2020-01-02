using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    public interface ITypeInfo
    {
        ModuleItemId? OuterTypeId { get; }
        ModuleItemId TypeId { get; }
        
        IReadOnlyList<string> GetTypeParams();
        TypeValue? GetBaseTypeValue();

        // TODO: 셋은 같은 이름공간을 공유한다. 서로 이름이 같은 것이 나오면 안된다 (체크하자)
        bool GetMemberTypeId(string name, [NotNullWhen(returnValue: true)] out ModuleItemId? outTypeId);
        bool GetMemberFuncId(Name memberFuncId, [NotNullWhen(returnValue: true)] out ModuleItemId? outFuncId);
        bool GetMemberVarId(Name name, [NotNullWhen(returnValue: true)] out ModuleItemId? outVarId);
    }
}
