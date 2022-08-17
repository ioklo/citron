using System;

using Citron.Collections;
using Citron.Infra;
using Citron.Module;

namespace Citron
{
    // TODO: IR0Evaluator로 옮긴다
    public class TypeContext
    {
        // index -> type
        ImmutableArray<SymbolId> env;

        public static readonly TypeContext Empty = new TypeContext(default);

        TypeContext(ImmutableArray<SymbolId> env)
        {
            this.env = env;
        }

        static void InnerMake(SymbolPath? path, ImmutableArray<SymbolId>.Builder builder)
        {
            if (path == null) return;

            InnerMake(path.Outer, builder);
            
            foreach (var typeArg in path.TypeArgs)
                builder.Add(typeArg);
        }

        public static TypeContext Make(SymbolPath? path)
        {
            var builder = ImmutableArray.CreateBuilder<SymbolId>();
            InnerMake(path, builder);
            return new TypeContext(builder.ToImmutable());
        }

        // G<List<T>>, T => int
        // Make((G<>, [(List<>, [T])]), [T => int])
        // => [(List<>, [int])]
        public static TypeContext Make(SymbolId symbolId, TypeContext typeContext)
        {
            return Make(typeContext.Apply(symbolId));
        }

        // Module.System.NS1.NS2.Type<int, bool>.Type2<short>.Func<string>
        public static TypeContext Make(SymbolId symbolId)
        {
            if (symbolId is ModuleSymbolId moduleSymbolId)
            {
                return Make(moduleSymbolId.Path);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        SymbolPath? ApplySymbolPath(SymbolPath? path)
        {
            if (path == null) return null;

            var appliedOuter = ApplySymbolPath(path.Outer);
            var appliedTypeArgs = ImmutableArray.CreateRange(path.TypeArgs, typeArg => Apply(typeArg));
            return new SymbolPath(appliedOuter, path.Name, appliedTypeArgs, path.ParamIds);
        }

        ModuleSymbolId ApplyModuleSymbolId(ModuleSymbolId moduleSymbolId)
        {
            var appliedPath = ApplySymbolPath(moduleSymbolId.Path);
            return new ModuleSymbolId(moduleSymbolId.ModuleName, appliedPath);
        }

        NullableSymbolId ApplyNullableSymbolId(NullableSymbolId nullableSymbolId)
        {
            var appliedInnerType = Apply(nullableSymbolId.InnerTypeId);
            return new NullableSymbolId(appliedInnerType);
        }

        SymbolId ApplyTypeVarSymbolId(TypeVarSymbolId typeVarSymbolId)
        {
            return env[typeVarSymbolId.Index];
        }

        TupleSymbolId ApplyTupleSymbolId(TupleSymbolId tupleSymbolId)
        {
            var appliedMemberVarIds = ImmutableArray.CreateRange(tupleSymbolId.MemberVarIds, 
                memberVarId => (Apply(memberVarId.TypeId), memberVarId.Name));

            return new TupleSymbolId(appliedMemberVarIds);
        }

        public ModuleSymbolId Apply(ModuleSymbolId moduleSymbolId) => ApplyModuleSymbolId(moduleSymbolId);

        public SymbolId Apply(SymbolId symbolId)
        {
            switch(symbolId)
            {
                case ModuleSymbolId moduleSymbolId:
                    return ApplyModuleSymbolId(moduleSymbolId);

                case TypeVarSymbolId typeVarSymbolId:
                    return ApplyTypeVarSymbolId(typeVarSymbolId);

                case NullableSymbolId nullableSymbolId:
                    return ApplyNullableSymbolId(nullableSymbolId);

                case VoidSymbolId:
                    return symbolId;

                case TupleSymbolId tupleSymbolId:
                    return ApplyTupleSymbolId(tupleSymbolId);

                case VarSymbolId:
                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}