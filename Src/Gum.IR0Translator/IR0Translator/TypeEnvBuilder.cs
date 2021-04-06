using System;
using Gum.Collections;

namespace Gum.IR0Translator
{
    internal class TypeEnvBuilder
    {
        ImmutableDictionary<TypeEnv.DepthIndex, TypeValue>.Builder builder;

        public TypeEnvBuilder()
        {
            builder = ImmutableDictionary.CreateBuilder<TypeEnv.DepthIndex, TypeValue>();
        }

        public TypeEnv Build()
        {
            return new TypeEnv(builder.ToImmutable());
        }

        public void Add(int depth, int index, TypeValue typeValue)
        {
            builder.Add(new TypeEnv.DepthIndex(depth, index), typeValue);
        }
    }
}