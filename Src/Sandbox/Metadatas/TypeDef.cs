using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    class TypeDef : MetadataComponent
    {
        public int TypeVarCount { get; private set; }
        public IReadOnlyList<MemberFuncDef> MemberFuncs { get { return memberFuncs; } }

        List<MemberFuncDef> memberFuncs = new List<MemberFuncDef>();
        List<MemberVarDef> memberVars = new List<MemberVarDef>();

        public TypeDef(Namespace ns, string name, int typeVarCount)
            : base(ns, name)
        {
            TypeVarCount = typeVarCount;
        }

        internal void AddFunc(string name, int typeVarCount, IType retType, IEnumerable<IType> paramTypes)
        {
            memberFuncs.Add(new MemberFuncDef(this, name, typeVarCount, retType, paramTypes));
        }

        internal void AddVar(IType varType, string name)
        {
            memberVars.Add(new MemberVarDef(this, varType, name));
        }
        
    }
}
