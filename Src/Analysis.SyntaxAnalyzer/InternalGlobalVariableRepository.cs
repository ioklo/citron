using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Citron.CompileTime;

namespace Citron.Analysis
{
    record InternalGlobalVarInfo(bool IsRef, ITypeSymbol TypeSymbol, Name Name);
    
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

        public void Update(InternalGlobalVariableRepository src, UpdateContext updateContext)
        {
            this.internalGlobalVarInfos = src.internalGlobalVarInfos;            
        }

        public InternalGlobalVarInfo? GetVariable(string name)
        {
            return internalGlobalVarInfos.GetValueOrDefault(name);
        }

        public void AddInternalGlobalVariable(bool bRef, ITypeSymbol typeValue, string name)
        {
            var globalVarInfo = new InternalGlobalVarInfo(bRef, typeValue, name);
            internalGlobalVarInfos = internalGlobalVarInfos.Add(name, globalVarInfo);
        }

        public bool HasVariable(string name)
        {
            return internalGlobalVarInfos.ContainsKey(name);
        }
    }
}
