using Citron.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;
using Gum.Infra;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    static class IR0Extensions
    {
        public static R.Path MakeRPath(this ISymbolNode symbol)
        {
            var symbolId = symbol.GetSymbolId();
            return symbolId.MakeRPath();
        }

        static R.Path.Normal MakeRPath(M.Name moduleName, SymbolPath? path)
        {
            if (path == null)
            {
                var rmoduleName = RItemFactory.MakeModuleName(moduleName);
                return new R.Path.Root(rmoduleName);
            }
            else
            {
                var outerPath = MakeRPath(moduleName, path.Outer);

                var rname = RItemFactory.MakeName(path.Name);

                var rparamHashEntries = path.ParamIds.MakeParamHashEntries(); // void Func<T>(T t) {} 일때, Func<int>의 paramIds는 [T] (Apply가 안된 상태)
                var rparamHash = new R.ParamHash(path.TypeArgs.Length, rparamHashEntries);

                var builder = ImmutableArray.CreateBuilder<R.Path>(path.TypeArgs.Length);
                foreach (var typeArg in path.TypeArgs)
                    builder.Add(typeArg.MakeRPath());

                return new R.Path.Nested(outerPath, rname, rparamHash, builder.MoveToImmutable());
            }
        }

        public static R.Path.Normal MakeRPath(this ModuleSymbolId symbolId)
        {
            return MakeRPath(symbolId.ModuleName, symbolId.Path);
        }

        public static R.Path MakeRPath(this SymbolId symbolId)
        {
            if (symbolId is ModuleSymbolId moduleSymbolId)
                return MakeRPath(moduleSymbolId.ModuleName, moduleSymbolId.Path);

            throw new NotImplementedException();
        }

        public static R.ParamHashEntry MakeParamHashEntry(this FuncParamId paramId)
        {
            return new R.ParamHashEntry(paramId.Kind.MakeRParamKind(), paramId.TypeId.MakeRPath());
        }

        public static ImmutableArray<R.ParamHashEntry> MakeParamHashEntries(this ImmutableArray<FuncParamId> paramIds)
        {
            var entries = ImmutableArray.CreateBuilder<R.ParamHashEntry>(paramIds.Length);
            foreach (var paramId in paramIds)
            {
                var paramHashEntry = paramId.MakeParamHashEntry();
                entries.Add(paramHashEntry);
            }

            return entries.MoveToImmutable();
        }
        
        public static R.ParamKind MakeRParamKind(this FuncParameterKind kind)
        {
            return kind switch
            {
                FuncParameterKind.Default => R.ParamKind.Default,
                FuncParameterKind.Ref => R.ParamKind.Ref,
                FuncParameterKind.Params => R.ParamKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }

        public static R.Param MakeRParam(this FuncParameter param)
        {
            var rparamKind = param.Kind.MakeRParamKind();
            var rname = RItemFactory.MakeName(param.Name);

            return new R.Param(rparamKind, param.Type.MakeRPath(), rname);
        }
    }
}
