using System;
using System.Diagnostics;
using Gum.Collections;
using Gum.Infra;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    class RItemFactory
    {
        public R.Path MakeTypeVar(int depth, int index)
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
            switch (name.Kind)
            {
                case M.SpecialName.Normal: return new R.Name.Normal(name.Text!);
                case M.SpecialName.IndexerGet: return new R.Name.IndexerGet();
                case M.SpecialName.IndexerSet: return new R.Name.IndexerSet();
                case M.SpecialName.AnonymousLambda: return new R.Name.AnonymousLambda(int.Parse(R.Name.Text!));
                case M.SpecialName.OpInc: return new R.Name.OpInc();
                case M.SpecialName.OpDec: return new R.Name.OpDec();
            }

            throw new UnreachableCodeException();
        }

        ImmutableArray<R.Path> MakeRTypes(ImmutableArray<TypeValue> typeValues)
        {
            return ImmutableArray.CreateRange(typeValues, typeValue => typeValue.GetRType());
        }

        public R.Path MakeStructType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, ImmutableArray<TypeValue> typeArgs)
        {
            var rmoduleName = MakeModuleName(moduleName);
            var rnsPath = MakeNamespacePath(namespacePath);
            var rname = MakeName(name);
            var rtypeArgs = MakeRTypes(typeArgs);
            
            return new R.Path.Root(rmoduleName, rnsPath, rname, rtypeArgs, R.ParamHash.None);
        }

        public R.Path MakeStructType(TypeValue outerType, M.Name name, ImmutableArray<TypeValue> typeArgs)
        {
            var routerType = outerType.GetRType() as R.Path.Normal;
            Debug.Assert(routerType != null);

            var rname = MakeName(name);
            var rtypeArgs = MakeRTypes(typeArgs);

            return new R.Path.Nested(routerType, rname, rtypeArgs, R.ParamHash.None);
        }

        public R.Path MakeMemberType(R.Path rtype, M.Name name, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeLambdaType(R.DeclId lambdaDeclId, R.Path returnRType, ImmutableArray<R.Path> paramRTypes)
        {
            return new R.AnonymousLambdaType(lambdaDeclId);
        }

        public R.Path MakeEnumElemType()
        {
            throw new NotImplementedException();
        }

        public R.Func MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Func MakeMemberFunc(R.Path outer, M.FuncInfo funcInfo, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.DeclId MakeDeclId(M.ModuleName value1, M.NamespacePath value2, M.FuncInfo funcInfo)
        {
            throw new NotImplementedException();
        }
    }
}