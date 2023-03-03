﻿using System;
using System.Diagnostics;
using Citron.Collections;
using Citron.Infra;
using R = Citron.IR0;
using M = Citron.Module;

namespace Citron.Analysis
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

                case M.Name.Singleton when name == M.Names.IndexerGet: return new R.Name.IndexerGet();
                case M.Name.Singleton when name == M.Names.IndexerSet: return new R.Name.IndexerSet();
                case M.Name.Singleton when name == M.Names.OpInc: return new R.Name.OpInc();
                case M.Name.Singleton when name == M.Names.OpDec: return new R.Name.OpDec();
                case M.Name.Singleton when name == M.Names.Constructor: return new R.Name.OpDec();

                case M.Name.ConstructorParam cp: return new R.Name.ConstructorParam(cp.Index);
                case M.Name.Anonymous anonymous: return new R.Name.Anonymous(anonymous.Index);
            }

            throw new UnreachableCodeException();
        }

        public static ImmutableArray<R.Path> MakeRTypes(ImmutableArray<ITypeSymbol> typeValues)
        {
            return ImmutableArray.CreateRange<ITypeSymbol, R.Path>(typeValues, typeValue => typeValue.MakeRPath());
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