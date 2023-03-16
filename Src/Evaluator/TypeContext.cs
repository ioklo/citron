using System;

using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

namespace Citron
{
    // 현재 실행하고 있는 도중의 타입 environment
    // body-space에서 존재, 
    // 최종적으로 TypeVar가 모두 치환되는 형태여야 한다
    // call할때 만들어질 것 같다(call할 대상의 SymbolId와, 타입인자로 확정)
    // 
    // TODO: IR0Evaluator로 옮긴다? => IR0한정 타입일 수도 있으므로?
    public class TypeContext
    {
        // index -> type
        ImmutableArray<TypeId> env;

        public static readonly TypeContext Empty = new TypeContext(default);

        TypeContext(ImmutableArray<TypeId> env)
        {
            this.env = env;
        }

        static void InnerMake(SymbolPath? path, ImmutableArray<TypeId>.Builder builder)
        {
            if (path == null) return;

            InnerMake(path.Outer, builder);
            
            foreach (var typeArg in path.TypeArgs)
                builder.Add(typeArg);
        }

        public static TypeContext Make(SymbolPath? path)
        {
            var builder = ImmutableArray.CreateBuilder<TypeId>();
            InnerMake(path, builder);
            return new TypeContext(builder.ToImmutable());
        }

        // G<List<T>>, T => int
        // Make((G<>, [(List<>, [T])]), [T => int])
        // => [(List<>, [int])]
        public static TypeContext Make(TypeId typeId, TypeContext typeContext)
        {
            return Make(typeContext.Apply(typeId));
        }

        // Module.System.NS1.NS2.Type<int, bool>.Type2<short>.Func<string>
        public static TypeContext Make(TypeId typeId)
        {
            switch(typeId)
            {
                case SymbolId symbolId: return Make(symbolId.Path);
                default: throw new NotImplementedException();
            }
        }

        SymbolPath? ApplySymbolPath(SymbolPath? path)
        {
            if (path == null) return null;

            var appliedOuter = ApplySymbolPath(path.Outer);
            var appliedTypeArgs = ImmutableArray.CreateRange<TypeId, TypeId>(path.TypeArgs, typeArg => Apply(typeArg));
            return new SymbolPath(appliedOuter, path.Name, appliedTypeArgs, path.ParamIds);
        }

        public SymbolId ApplySymbol(SymbolId symbolId)
        {
            var appliedPath = ApplySymbolPath(symbolId.Path);
            return new SymbolId(symbolId.ModuleName, appliedPath);
        }

        NullableTypeId ApplyNullable(NullableTypeId id)
        {
            var appliedId = Apply(id.InnerTypeId);
            return new NullableTypeId(appliedId);
        }

        TypeId ApplyTypeVar(TypeVarTypeId id)
        {
            return env[id.Index];
        }

        TupleTypeId ApplyTuple(TupleTypeId id)
        {
            var appliedIds = ImmutableArray.CreateRange(id.MemberVarIds, 
                memberVarId => new TupleMemberVarId(Apply(memberVarId.TypeId), memberVarId.Name));

            return new TupleTypeId(appliedIds);
        }

        public TypeId Apply(TypeId typeId)
        {
            switch(typeId)
            {
                case SymbolId symbolId:
                    return ApplySymbol(symbolId);

                case TypeVarTypeId typeVarId:
                    return ApplyTypeVar(typeVarId);

                case NullableTypeId nullableId:
                    return ApplyNullable(nullableId);

                case VoidTypeId voidId:
                    return voidId;

                case TupleTypeId tupleId:
                    return ApplyTuple(tupleId);

                case VarTypeId:
                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}