using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using Citron.Module;
using System.Diagnostics;

namespace Citron.Symbol
{   
    public partial class StructMemberVarDeclSymbol : IDeclSymbolNode
    {   
        StructDeclSymbol outer;
        Accessor accessor;
        bool bStatic;
        IType declType;
        Name name;

        public StructMemberVarDeclSymbol(StructDeclSymbol outer, Accessor accessor, bool bStatic, IType declType, Name name)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.bStatic = bStatic;
            this.declType = declType;
            this.name = name;
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IType GetDeclType()
        {   
            return declType;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }
    }
}