using Pretune;
using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{
    public record struct FuncParamId(FuncParameterKind Kind, FuncParamTypeId TypeId) : ISerializable
    {
        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeString(nameof(Kind), Kind.ToString());
            context.SerializeRef(nameof(TypeId), TypeId);
        }
    }

    public abstract record class FuncParamTypeId : ISerializable
    {
        public abstract void DoSerialize(ref SerializeContext context);

        public record Symbol(SymbolId Id) : FuncParamTypeId
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeRef(nameof(Id), Id);
            }
        }

        public record Tuple(ImmutableArray<FuncParamTypeId> MemberTypeIds) : FuncParamTypeId
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeRefArray(nameof(MemberTypeIds), MemberTypeIds);
            }
        }

        public record Nullable(FuncParamTypeId InnerTypeId) : FuncParamTypeId
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeRef(nameof(InnerTypeId), InnerTypeId);
            }
        }

        public record Void : FuncParamTypeId
        {
            public override void DoSerialize(ref SerializeContext context)
            {
            }
        }

        public record TypeVar(int Index) : FuncParamTypeId
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeInt(nameof(Index), Index);
            }
        }
    }

    public static class FuncParamIdExtensions
    {
        public static ImmutableArray<FuncParamId> MakeFuncParamIds(this ImmutableArray<FuncParameter> funcParams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParamId>(funcParams.Length);

            foreach (var funcParam in funcParams)
            {
                var kind = funcParam.Kind;
                var typeId = funcParam.Type.MakeFuncParamTypeId();

                builder.Add(new FuncParamId(kind, typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}