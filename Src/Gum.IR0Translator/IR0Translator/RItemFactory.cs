using System;
using System.Diagnostics;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    class RItemFactory : IPure
    {
        public void EnsurePure()
        {
        }

        public static R.ModuleName MakeModuleName(M.ModuleName moduleName)
        {
            return new R.ModuleName(moduleName.Text);
        }

        public static R.Name MakeName(M.Name name)
        {
            switch (name.Kind)
            {
                case M.SpecialName.Normal: return new R.Name.Normal(name.Text!);
                case M.SpecialName.IndexerGet: return new R.Name.IndexerGet();
                case M.SpecialName.IndexerSet: return new R.Name.IndexerSet();
                case M.SpecialName.AnonymousLambda: return new R.Name.Anonymous(new R.AnonymousId(int.Parse(name.Text!)));
                case M.SpecialName.OpInc: return new R.Name.OpInc();
                case M.SpecialName.OpDec: return new R.Name.OpDec();
            }

            throw new UnreachableCodeException();
        }

        public static ImmutableArray<R.Path> MakeRTypes(ImmutableArray<TypeValue> typeValues)
        {
            return ImmutableArray.CreateRange(typeValues, typeValue => typeValue.GetRPath());
        }        
        
        public R.Path MakeMemberType(R.Path rtype, M.Name name, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeLambdaType(R.Path.Nested lambda)
        {
            return new R.Path.AnonymousLambdaType(lambda);
        }

        public R.Path MakeEnumElemType()
        {
            throw new NotImplementedException();
        }

        public R.Path MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeMemberFunc(R.Path outer, M.FuncInfo funcInfo, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeTupleType(ImmutableArray<R.TypeAndName> elems)
        {
            return new R.Path.TupleType(elems);
        }
    }
}