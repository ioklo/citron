using System;
using Gum.Collections;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Infra;

namespace Gum.Analysis
{
    public record ClassMemberFuncDeclSymbol : IDeclSymbolNode
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
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.GetValue().MakeMParamTypes());
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Nested outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parameters.GetValue().MakeParamHashEntries());
            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }
    }
}
