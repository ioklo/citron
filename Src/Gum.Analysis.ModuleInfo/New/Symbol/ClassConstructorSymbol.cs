using Gum.Collections;
using System;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class ClassConstructorSymbol : ISymbolNode
    {
        SymbolFactory factory;
        ClassSymbol outer;
        ClassConstructorDeclSymbol decl;

        internal ClassConstructorSymbol(SymbolFactory factory, ClassSymbol outer, ClassConstructorDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var typeEnv = outer.GetTypeEnv();
            var param = decl.GetParameter(index);
            return param.Apply(typeEnv);
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
            return outer.GetTypeEnv();
        }
        
        public R.Path.Nested MakeRPath()
        {
            var outerPath = outer.MakeRPath();

            // path의 이름 부분은 apply 되지 않은 상태여야 한다
            // decl이 직접 parameter로부터 path를 만들어 내는 것이 나은것 같다
            return decl.MakeRPath(outerPath);
        }

        public ClassConstructorSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeClassConstructor(appliedOuter, decl);
        }

        public int GetTotalTypeParamCount()
        {
            return decl.GetTotalTypeParamCount();
        }

        public M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            // TODO: 추후 전용 Exception을 새로 만든다
            throw new InvalidOperationException();
        }        

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }

        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    }
}