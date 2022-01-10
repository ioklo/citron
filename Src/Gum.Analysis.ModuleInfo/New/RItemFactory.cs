using System;
using System.Diagnostics;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class RItemFactory
    {
        public static R.ModuleName MakeModuleName(M.Name moduleName)
        {
            if (moduleName is M.Name.Normal normalName)
                return new R.ModuleName(normalName.Text);

            throw new UnreachableCodeException();
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

        public static ImmutableArray<R.Path> MakeRTypes(ImmutableArray<ITypeSymbolNode> typeValues)
        {
            return ImmutableArray.CreateRange<ITypeSymbolNode, R.Path>(typeValues, typeValue => typeValue.MakeRPath());
        }        
        
        public R.Path MakeMemberType(R.Path rtype, M.Name name, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeEnumElemType()
        {
            throw new NotImplementedException();
        }

        public R.Path MakeGlobalFunc(DeclSymbolPath outerPath, M.GlobalFuncDecl funcDecl, ImmutableArray<R.Path> rtypeArgs)
        {
            throw new NotImplementedException();
        }

        public R.Path MakeTupleType(ImmutableArray<R.TupleTypeElem> elems)
        {
            return new R.Path.TupleType(elems);
        }
    }
}