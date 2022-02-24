using System;
using Citron.Collections;
using M = Citron.CompileTime;
using Citron.Infra;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Analysis
{
    public record ClassMemberFuncDeclSymbol : IFuncDeclSymbol
    {
        IHolder<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        IHolder<FuncReturn> @return;
        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parameters;        
        bool bStatic;

        public ClassMemberFuncDeclSymbol(
            IHolder<ClassDeclSymbol> outer, 
            M.AccessModifier accessModifier, 
            IHolder<FuncReturn> @return,
            M.Name name,
            ImmutableArray<string> typeParams,
            IHolder<ImmutableArray<FuncParameter>> parameters,
            bool bStatic)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.@return = @return;
            this.name = name;
            this.typeParams = typeParams;
            this.parameters = parameters;
            this.bStatic = bStatic;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public int GetParameterCount()
        {
            return parameters.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters.GetValue()[index];
        }

        public FuncReturn GetReturn()
        {
            return @return.GetValue();
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }
        
        public bool IsStatic()
        {
            return bStatic;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.GetValue().MakeFuncParamIds());
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberFunc(this);
        }
    }
}
