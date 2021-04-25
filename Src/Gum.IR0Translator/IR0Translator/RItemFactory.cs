using System;
using Gum.Collections;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    class RItemFactory
    {
        public R.Type MakeTypeVar(int depth, int index)
        {
            return new R.TypeVar(depth, index);
        }

        R.ModuleName MakeModuleName(M.ModuleName moduleName)
        {
            return new R.ModuleName(moduleName.Text);
        }        

        R.NamespacePath MakeNamespacePath(M.NamespacePath nsPath)
        {
            var rentries = ImmutableArray.CreateRange(nsPath.Entries, entry => new R.NamespaceName(entry.Value));
            return new R.NamespacePath(rentries);
        }

        R.Name MakeName(M.Name name)
        {
            return new R.Name((R.SpecialName)name.Kind, name.Text);
        }

        ImmutableArray<R.Type> MakeRTypes(ImmutableArray<TypeValue> typeValues)
        {
            return ImmutableArray.CreateRange(typeValues, typeValue => typeValue.GetRType());
        }

        public R.StructType MakeStructType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, ImmutableArray<TypeValue> typeArgs)
        {
            var rmoduleName = MakeModuleName(moduleName);
            var rnsPath = MakeNamespacePath(namespacePath);
            var rname = MakeName(name);
            var rtypeArgs = MakeRTypes(typeArgs);

            var outerType = new R.RootOuterType(rmoduleName, rnsPath);
            return new R.StructType(outerType, rname, rtypeArgs);
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
    }
}