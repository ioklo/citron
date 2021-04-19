using System;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    public class RItemFactory
    {
        public R.Type MakeTypeVar(int depth, int index)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeGlobalType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeMemberType(M.Type outerRType, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeLambdaType(R.DeclId lambdaDeclId, Type returnRType, ImmutableArray<R.Type> paramRTypes)
        {
            return new R.AnonymousLambdaType(lambdaDeclId);
        }

        public R.Type MakeEnumElemType()
        {
            throw new NotImplementedException();
        }

        public R.Func MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Func MakeMemberFunc(Type outer, M.FuncInfo funcInfo, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }
    }
}