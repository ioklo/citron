using System;
using Gum.Collections;
using Gum.CompileTime;
using Gum.IR0;
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

        public R.Type MakeMemberType(R.Type rtype, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Type MakeLambdaType(R.DeclId lambdaDeclId, R.Type returnRType, ImmutableArray<R.Type> paramRTypes)
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

        public R.Func MakeMemberFunc(R.Type outer, M.FuncInfo funcInfo, ImmutableArray<R.Type> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public DeclId MakeDeclId(M.ModuleName value1, M.NamespacePath value2, FuncInfo funcInfo)
        {
            throw new NotImplementedException();
        }

        public TypeContext MakeTypeContext()
        {
            throw new NotImplementedException();
        }
    }
}