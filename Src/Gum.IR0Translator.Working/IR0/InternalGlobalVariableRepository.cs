using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    class InternalGlobalVariableRepository : IInternalGlobalVariableRepository
    {
        // global variable        
        Dictionary<Name, InternalGlobalVarInfo> internalGlobalVarInfos;

        public InternalGlobalVariableRepository()
        {
            internalGlobalVarInfos = new Dictionary<Name, InternalGlobalVarInfo>();
        }

        public InternalGlobalVarInfo? GetVariable(Name name)
        {
            return internalGlobalVarInfos.GetValueOrDefault(name);            
        }

        public void AddInternalGlobalVariable(Name name, TypeValue typeValue)
        {
            var globalVarInfo = new InternalGlobalVarInfo(name, typeValue);
            internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(Name name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
