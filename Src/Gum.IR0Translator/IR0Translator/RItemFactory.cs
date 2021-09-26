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
            switch (name)
            {
                case M.Name.Normal normalName: return new R.Name.Normal(normalName.Text);

                case M.Name.Singleton when name == M.Name.IndexerGet: return new R.Name.IndexerGet();
                case M.Name.Singleton when name == M.Name.IndexerSet: return new R.Name.IndexerSet();
                case M.Name.Singleton when name == M.Name.OpInc: return new R.Name.OpInc();
                case M.Name.Singleton when name == M.Name.OpDec: return new R.Name.OpDec();
                case M.Name.Singleton when name == M.Name.Constructor: return new R.Name.OpDec();

                case M.Name.ConstructorParam cp: return new R.Name.ConstructorParam(cp.Index);
                case M.Name.Anonymous anonymous: return new R.Name.Anonymous(anonymous.Index);
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

        public R.Path MakeTupleType(ImmutableArray<R.TupleTypeElem> elems)
        {
            return new R.Path.TupleType(elems);
        }
    }
}