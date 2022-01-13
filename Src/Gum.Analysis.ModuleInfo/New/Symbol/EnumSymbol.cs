using System;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class EnumSymbol : ITypeSymbolNode
    {
        SymbolFactory factory;

        ISymbolNode outer;
        EnumDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;

        TypeEnv typeEnv;

        internal EnumSymbol(SymbolFactory factory, ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.MakeRPath(), rname, new R.ParamHash(decl.GetTypeParamCount(), default), rtypeArgs);
        }

        public EnumSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeEnum(appliedOuter, decl, appliedTypeArgs);
        }

        //
        public MemberQueryResult GetMember(M.Name memberName, int typeParamCount) 
        {
            // shortcut
            if (typeParamCount != 0)
                return MemberQueryResult.NotFound.Instance;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return MemberQueryResult.NotFound.Instance;

            var elem = factory.MakeEnumElem(this, elemDecl);

            return new MemberQueryResult.EnumElem(elem);
        }

        public EnumElemSymbol? GetElement(string name)
        {
            var elemDecl = decl.GetElem(new M.Name.Normal(name));
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }

        public ITypeSymbolNode? GetMemberType(M.Name memberName, ImmutableArray<ITypeSymbolNode> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }

        public R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        { 
            throw new InvalidOperationException();
        }
        
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbolNode ITypeSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ITypeDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }        
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }        
        
        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }
    }
}
