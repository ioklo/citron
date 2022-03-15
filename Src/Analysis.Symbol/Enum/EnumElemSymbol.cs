using Citron.Collections;
using System.Diagnostics;

using M = Citron.CompileTime;
using Pretune;
using System;
using Citron.Infra;

namespace Citron.Analysis
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
            var memberVarCount = decl.GetMemberVarCount();

            var builder = ImmutableArray.CreateBuilder<FuncParameter>(memberVarCount);
            for(int i = 0; i < memberVarCount; i++)
            {
                var memberVar = decl.GetMemberVar(i);

                var typeEnv = outer.GetTypeEnv();
                var declType = memberVar.GetDeclType();
                
                var appliedDeclType = declType.Apply(typeEnv);

                builder.Add(new FuncParameter(M.FuncParameterKind.Default, appliedDeclType, decl.GetName())); // TODO: EnumElemFields에 ref를 지원할지
            }

            return builder.MoveToImmutable();
        }

        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
        {
            if (typeParamCount != 0) return SymbolQueryResult.NotFound.Instance;

            int memberVarCount = decl.GetMemberVarCount();

            for(int i = 0; i < memberVarCount; i++)
            {
                var varDecl = decl.GetMemberVar(i);

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

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public EnumElemMemberVarSymbol GetMemberVar(int i)
        {
            var memberVarDecl = decl.GetMemberVar(i);
            return factory.MakeEnumElemMemberVar(this, memberVarDecl);
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
