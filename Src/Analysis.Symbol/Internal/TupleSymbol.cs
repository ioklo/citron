using System;
using Citron.Collections;
using M = Citron.CompileTime;
using Pretune;

namespace Citron.Analysis
{
    public class TupleSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        ImmutableArray<TupleMemberVarSymbol> memberVars;

        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        
        internal TupleSymbol(SymbolFactory factory, ImmutableArray<TupleMemberVarSymbol> memberVars)
        {
            this.factory = factory;            
            this.memberVars = memberVars;
        }

        public TupleSymbol Apply(TypeEnv typeEnv)
        {
            var appliedMemberVars = ImmutableArray.CreateRange(memberVars, memberVar => memberVar.Apply(typeEnv));
            return factory.MakeTuple(appliedMemberVars);
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitTuple(this);
        }

        public ITypeDeclSymbol? GetDeclSymbolNode()
        {
            return null;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
        }

        public TupleMemberVarSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
        {
            foreach(var memberVar in memberVars)
            {
                if (memberName.Equals(memberVar.GetName()))
                {
                    if (typeParamCount != 0)
                        return SymbolQueryResult.Error.VarWithTypeArg.Instance;

                    return new SymbolQueryResult.TupleMemberVar(memberVar);
                }
            }

            return SymbolQueryResult.NotFound.Instance;
        }
    }
}
