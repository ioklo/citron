using Gum.Infra;
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

    class InternalGlobalVariableRepository : IMutable<InternalGlobalVariableRepository>
    {
        // global variable        
        Dictionary<M.Name, InternalGlobalVarInfo> internalGlobalVarInfos;

        public InternalGlobalVariableRepository()
        {
            internalGlobalVarInfos = new Dictionary<M.Name, InternalGlobalVarInfo>();
        }

        InternalGlobalVariableRepository(InternalGlobalVariableRepository other, CloneContext cloneContext)
        {
            this.internalGlobalVarInfos = new Dictionary<M.Name, InternalGlobalVarInfo>(other.internalGlobalVarInfos);
        }

        public InternalGlobalVariableRepository Clone(CloneContext context)
        {
            return new InternalGlobalVariableRepository(this, context);
        }        

        public void Update(InternalGlobalVariableRepository src, UpdateContext updateContext)
        {
            this.internalGlobalVarInfos.Clear();
            foreach (var (key, value) in src.internalGlobalVarInfos)
                this.internalGlobalVarInfos.Add(key, value);
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
