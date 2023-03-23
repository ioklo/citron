using Pretune;
using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{
    public record struct FuncParamId(FuncParameterKind Kind, TypeId TypeId) : ISerializable
    {
        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeString(nameof(Kind), Kind.ToString());
            context.SerializeRef(nameof(TypeId), TypeId);
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
                var typeId = funcParam.Type.GetTypeId();

                builder.Add(new FuncParamId(kind, typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}