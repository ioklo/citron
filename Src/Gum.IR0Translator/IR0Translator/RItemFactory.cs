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
            return new R.TypeVar(depth, index);
        }        

        public R.Type MakeStructType(R.OuterType outerType, M.Name name, ImmutableArray<R.Type> rtypeArgs)
        {
            return new R.StructType(outerType, name, typeContext);
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

        public R.DeclId MakeDeclId(M.ModuleName value1, M.NamespacePath value2, M.FuncInfo funcInfo)
        {
            throw new NotImplementedException();
        }

        public R.TypeContext MakeTypeContext()
        {
            throw new NotImplementedException();
        }
    }
}