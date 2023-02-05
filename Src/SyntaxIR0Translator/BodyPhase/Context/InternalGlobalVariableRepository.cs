using Citron.Collections;
using Citron.Infra;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Symbol;

namespace Citron.Analysis
{
    record class InternalGlobalVarInfo(bool IsRef, IType Type, Name Name);
    
    class InternalGlobalVariableRepository : IMutable<InternalGlobalVariableRepository>
    {
        // global variable
        ImmutableDictionary<string, InternalGlobalVarInfo> internalGlobalVarInfos;

        public InternalGlobalVariableRepository()
        {
            internalGlobalVarInfos = ImmutableDictionary<string, InternalGlobalVarInfo>.Empty;
        }

        InternalGlobalVariableRepository(InternalGlobalVariableRepository other, CloneContext cloneContext)
        {
            this.internalGlobalVarInfos = other.internalGlobalVarInfos;
        }

        public InternalGlobalVariableRepository Clone(CloneContext context)
        {
            return new InternalGlobalVariableRepository(this, context);
        }        

        //public void Update(InternalGlobalVariableRepository src, UpdateContext updateContext)
        //{
        //    this.internalGlobalVarInfos = src.internalGlobalVarInfos;            
        //}

        public InternalGlobalVarInfo? GetVariable(string name)
        {
            return internalGlobalVarInfos.GetValueOrDefault(name);
        }

        public void AddInternalGlobalVariable(bool bRef, IType typeValue, string name)
        {
            var globalVarInfo = new InternalGlobalVarInfo(bRef, typeValue, new Name.Normal(name));
            internalGlobalVarInfos = internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(string name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
