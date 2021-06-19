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
        public bool IsRef { get; }
        public TypeValue TypeValue { get; }
        public M.Name Name { get; }        

        public InternalGlobalVarInfo(bool bRef, TypeValue typeValue, M.Name name) 
        {
            IsRef = bRef;
            TypeValue = typeValue;
            Name = name;
        }
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

        public void AddInternalGlobalVariable(bool bRef, TypeValue typeValue, M.Name name)
        {
            var globalVarInfo = new InternalGlobalVarInfo(bRef, typeValue, name);
            internalGlobalVarInfos = internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(M.Name name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
