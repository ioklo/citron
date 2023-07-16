using Pretune;
using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{
    public record struct FuncParamId(TypeId TypeId) : ISerializable
    {
        void ISerializable.DoSerialize(ref SerializeContext context)
        {   
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
                var typeId = funcParam.Type.GetTypeId();

                builder.Add(new FuncParamId(typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}