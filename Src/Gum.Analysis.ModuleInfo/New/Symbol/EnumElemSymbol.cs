using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using System;

namespace Gum.Analysis
{
    // S.First, S.Second(int i, short s)    
    public partial class EnumElemSymbol : ITypeSymbolNode
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

        public R.Path.Nested MakeRPath()
        {
            var router = outer.MakeRPath();
            var rname = RItemFactory.MakeName(decl.GetName());
            Debug.Assert(router != null);

            return new R.Path.Nested(router, rname, R.ParamHash.None, default);
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

        public MemberQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (typeParamCount != 0) return MemberQueryResult.NotFound.Instance;

            foreach (var varDecl in decl.GetMemberVarDecls())
            {
                if (memberName.Equals(varDecl.GetName()))
                {
                    if (typeParamCount != 0)
                        return MemberQueryResult.Error.VarWithTypeArg.Instance;

                    var memberVar = factory.MakeEnumElemMemberVar(this, varDecl);
                    return new MemberQueryResult.EnumElemMemberVar(memberVar);
                }
            }

            return MemberQueryResult.NotFound.Instance;
        }

        public R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            return new R.EnumElemMemberLoc(instance, member);
        }

        

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
            return outer.GetTypeEnv();
        }        

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }

        ITypeSymbolNode ITypeSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    }
}
