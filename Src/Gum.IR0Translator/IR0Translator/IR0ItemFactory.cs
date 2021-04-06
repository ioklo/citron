using System;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    class IR0ItemFactory
    {
        public R.Type MakeTypeVar(int depth, int index)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeGlobalType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeMemberType(R.Type outerRType, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeLambdaType(int lambdaId, R.Type returnRType, ImmutableArray<R.Type> paramRTypes)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeEnumElemType()
        {
            throw new NotImplementedException();
        }

        public R.Func MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Func MakeMemberFunc(R.Type outer, M.FuncInfo funcInfo, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }
    }
}