using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    class InternalGlobalVarInfo
    {
        public M.Name Name { get; }
        public TypeValue TypeValue { get; }

        public InternalGlobalVarInfo(M.Name name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
    }

    class InternalGlobalVariableRepository
    {
        // global variable        
        Dictionary<M.Name, InternalGlobalVarInfo> internalGlobalVarInfos;

        public InternalGlobalVariableRepository()
        {
            internalGlobalVarInfos = new Dictionary<M.Name, InternalGlobalVarInfo>();
        }

        public InternalGlobalVarInfo? GetVariable(M.Name name)
        {
            return internalGlobalVarInfos.GetValueOrDefault(name);            
        }

        public void AddInternalGlobalVariable(M.Name name, TypeValue typeValue)
        {
            var globalVarInfo = new InternalGlobalVarInfo(name, typeValue);
            internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(M.Name name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
