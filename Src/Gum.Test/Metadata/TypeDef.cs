using Gum.Test.TypeInst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
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

        internal void AddFunc(string name, int typeVarCount, ITypeInst retType, IEnumerable<ITypeInst> paramTypes)
        {
            memberFuncs.Add(new MemberFuncDef(this, name, typeVarCount, retType, paramTypes));
        }

        internal void AddVar(ITypeInst varType, string name)
        {
            memberVars.Add(new MemberVarDef(this, varType, name));
        }
        
    }
}
