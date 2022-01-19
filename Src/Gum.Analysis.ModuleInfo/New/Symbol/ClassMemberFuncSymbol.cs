using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // X<int>.Y<short>.F_T_int_int<S>
    public class ClassMemberFuncSymbol : IFuncSymbol
    {
        SymbolFactory factory;

        // X<int>.Y<short>
        ClassSymbol outer;

        // F_int_int
        ClassMemberFuncDeclSymbol decl;

        ImmutableArray<ITypeSymbol> typeArgs;

        // cached typeEnv
        TypeEnv typeEnv;

        // for return type covariance        
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal ClassMemberFuncSymbol(SymbolFactory factory, ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;
            this.typeEnv = outer.GetTypeEnv().AddTypeArgs(typeArgs);
        }
        
        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(typeEnv);
        }

        public FuncReturn GetReturn()
        {
            var ret = decl.GetReturn();
            return ret.Apply(typeEnv);
        }        

        public ClassMemberFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeClassMemberFunc(appliedOuter, decl, appliedTypeArgs);
        }
        
        public bool CheckAccess(ITypeSymbol? userType)
        {
            var accessModifier = decl.GetAccessModifier();
            
            switch (accessModifier)
            {
                case M.AccessModifier.Public: return true;
                case M.AccessModifier.Protected: throw new NotImplementedException();
                case M.AccessModifier.Private:
                    {
                        // NOTICE: ConstructorValue, MemberVarValue에도 같은 코드가 있다 
                        if (userType == null) return false;

                        // decl끼리 비교한다
                        var outerDecl = outer.GetDeclSymbolNode();
                        var userTypeDecl = userType.GetDeclSymbolNode();

                        if (userTypeDecl.Equals(outerDecl))
                            return true;

                        return userTypeDecl.IsDescendantOf(outerDecl);
                    }

                default: throw new UnreachableCodeException();
            }
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }
        
        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return typeArgs;
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }
    }
}
