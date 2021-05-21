using System;
using Gum.Collections;

namespace Gum.IR0Translator
{
    internal class TypeEnvBuilder
    {
        ImmutableArray<TypeValue>.Builder builder;

        public TypeEnvBuilder()
        {
            builder = ImmutableArray.CreateBuilder<TypeValue>();
        }

        public TypeEnv Build()
        {
            return new TypeEnv(builder.ToImmutable());
        }

        public void Add(TypeValue typeValue)
        {
            builder.Add(typeValue);
        }
    }
}