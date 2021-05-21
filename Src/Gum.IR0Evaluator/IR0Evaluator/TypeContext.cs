using Gum.Collections;
using Gum.Infra;
using System;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    class TypeContext
    {
        // index -> type
        ImmutableArray<R.Path> env;

        TypeContext(ImmutableArray<R.Path> env)
        {
            this.env = env;
        }

        static void InnerMake(R.Path.Normal path, ImmutableArray<R.Path>.Builder builder)
        {
            if (path is R.Path.Root) return;

            else if (path is R.Path.Nested nestedPath)
            {
                InnerMake(nestedPath.Outer, builder);

                foreach (var typeArg in nestedPath.TypeArgs)
                    builder.Add(typeArg);

                return;
            }

            throw new UnreachableCodeException();
        }


        // Module.System.NS1.NS2.Type<int, bool>.Type2<short>.Func<string>
        public static TypeContext Make(R.Path.Normal path)
        {
            var builder = ImmutableArray.CreateBuilder<R.Path>();
            InnerMake(path, builder);
            return new TypeContext(builder.ToImmutable());            
        }

        public R.Path Apply(R.Path path)
        {
            // TODO: path를 다 돌아서 TypeVar를 치환한다
            return path;
        }
    }
}