﻿using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    // X<int>.Y<short>.F_T_int_int<S>
    public class ClassMemberFuncSymbol : ISymbolNode
    {
        SymbolFactory factory;

        // X<int>.Y<short>
        ClassSymbol outer;

        // F_int_int
        ClassMemberFuncDeclSymbol decl;

        ImmutableArray<ITypeSymbolNode> typeArgs;

        // cached typeEnv
        TypeEnv typeEnv;
        
        internal ClassMemberFuncSymbol(SymbolFactory factory, ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
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

        public R.Path.Nested MakeRPath()
        {
            var outerPath = outer.MakeRPath();
            var rtypeArgs = ImmutableArray.CreateRange<ITypeSymbolNode, R.Path>(typeArgs, typeArg => typeArg.MakeRPath());

            return decl.MakeRPath(outerPath, rtypeArgs);
        }

        public bool CheckAccess(ITypeSymbolNode? userType)
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

        // for return type covariance
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            // 자식아이템을 가질 수 없으므로 ChildTypeId를 만들 수 없다
            throw new InvalidOperationException();
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }
    }
}
