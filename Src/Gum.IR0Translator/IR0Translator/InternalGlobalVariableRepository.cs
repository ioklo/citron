using Gum.Collections;
using Gum.Infra;
using System;
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
        ImmutableDictionary<M.Name, InternalGlobalVarInfo> internalGlobalVarInfos;

        public InternalGlobalVariableRepository()
        {
            internalGlobalVarInfos = ImmutableDictionary<M.Name, InternalGlobalVarInfo>.Empty;
        }

        InternalGlobalVariableRepository(InternalGlobalVariableRepository other, CloneContext cloneContext)
        {
            this.internalGlobalVarInfos = other.internalGlobalVarInfos;
        }

        public InternalGlobalVariableRepository Clone(CloneContext context)
        {
            return new InternalGlobalVariableRepository(this, context);
        }        

        public void Update(InternalGlobalVariableRepository src, UpdateContext updateContext)
        {
            this.internalGlobalVarInfos = src.internalGlobalVarInfos;            
        }

        public InternalGlobalVarInfo? GetVariable(M.Name name)
        {
            return internalGlobalVarInfos.GetValueOrDefault(name);            
        }

        public void AddInternalGlobalVariable(M.Name name, TypeValue typeValue)
        {
            var globalVarInfo = new InternalGlobalVarInfo(name, typeValue);
            internalGlobalVarInfos = internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(M.Name name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
