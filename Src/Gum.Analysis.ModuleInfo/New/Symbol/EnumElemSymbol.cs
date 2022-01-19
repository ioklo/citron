using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;
using Pretune;
using System;

namespace Gum.Analysis
{
    // S.First, S.Second(int i, short s)    
    public partial class EnumElemSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        EnumSymbol outer;
        EnumElemDeclSymbol decl;

        internal EnumElemSymbol(SymbolFactory factory, EnumSymbol outer, EnumElemDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public bool IsStandalone()
        {
            return decl.IsStandalone();
        }

        public EnumElemSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeEnumElem(appliedOuter, decl);
        }

        public ImmutableArray<FuncParameter> GetConstructorParamTypes()
        {
            var varDecls = decl.GetMemberVarDecls();

            var builder = ImmutableArray.CreateBuilder<FuncParameter>(varDecls.Length);
            foreach(var varDecl in varDecls)
            {
                var typeEnv = outer.GetTypeEnv();
                var declType = varDecl.GetDeclType();
                
                var appliedDeclType = declType.Apply(typeEnv);

                builder.Add(new FuncParameter(FuncParameterKind.Default, appliedDeclType, decl.GetName())); // TODO: EnumElemFields에 ref를 지원할지
            }

            return builder.MoveToImmutable();
        }

        public SymbolQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (typeParamCount != 0) return SymbolQueryResult.NotFound.Instance;

            foreach (var varDecl in decl.GetMemberVarDecls())
            {
                if (memberName.Equals(varDecl.GetName()))
                {
                    if (typeParamCount != 0)
                        return SymbolQueryResult.Error.VarWithTypeArg.Instance;

                    var memberVar = factory.MakeEnumElemMemberVar(this, varDecl);
                    return new SymbolQueryResult.EnumElemMemberVar(memberVar);
                }
            }

            return SymbolQueryResult.NotFound.Instance;
        }

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public EnumSymbol GetOuter()
        {
            return outer;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }        

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public SymbolQueryResult QueryMember(M.Name name, int typeParamCount)
        {
            return SymbolQueryResult.NotFound.Instance;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();
    }
}
