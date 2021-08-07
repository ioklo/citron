using Gum.Collections;
using Gum.Infra;
using System;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public class TypeContext
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

        public R.Path.Nested Apply_Nested(R.Path.Nested nestedPath)
        {
            var appliedOuter = Apply_Normal(nestedPath.Outer);
            var builder = ImmutableArray.CreateBuilder<R.Path>(nestedPath.TypeArgs.Length);
            foreach (var typeArg in nestedPath.TypeArgs)
            {
                var appliedTypeArg = Apply(typeArg);
                builder.Add(appliedTypeArg);
            }

            // ParamHash는 건드리지 않는다
            return new R.Path.Nested(appliedOuter, nestedPath.Name, nestedPath.ParamHash, builder.MoveToImmutable());
        }
        public R.Path.Normal Apply_Normal(R.Path.Normal normalPath)
        {
            switch (normalPath)
            {
                case R.Path.Root: 
                    return normalPath;

                case R.Path.Nested nestedPath:
                    return Apply_Nested(nestedPath);                    

                default:
                    throw new UnreachableCodeException();
            }
        }

        public R.Path Apply(R.Path path)
        {
            switch(path)
            {
                // Reserved
                case R.Path.TupleType tuplePath:
                    {
                        var builder = ImmutableArray.CreateBuilder<R.TupleTypeElem>(tuplePath.Elems.Length);
                        foreach (var elem in tuplePath.Elems)
                        {
                            var appliedType = Apply(elem.Type);
                            var appliedElem = new R.TupleTypeElem(appliedType, elem.Name);
                            builder.Add(appliedElem);
                        }
                        return new R.Path.TupleType(builder.MoveToImmutable());
                    }

                case R.Path.TypeVarType typeVarPath:
                    return env[typeVarPath.Index];

                case R.Path.VoidType:
                    return path;

                case R.Path.BoxType boxPath:
                    {
                        var appliedType = Apply(boxPath.Type);
                        return new R.Path.BoxType(appliedType);
                    }

                case R.Path.GenericRefType genericRefPath:
                    {
                        // TODO: TRef는 이럴때 녹아야 하지 않는가
                        var appliedType = Apply(genericRefPath);
                        return new R.Path.GenericRefType(appliedType);
                    }

                case R.Path.FuncType funcPath:
                        throw new NotImplementedException();                        

                case R.Path.NullableType nullablePath:
                    {
                        var appliedType = Apply(nullablePath.Type);
                        return new R.Path.NullableType(appliedType);
                    }

                // Normal
                case R.Path.Normal normalPath:
                    return Apply_Normal(normalPath);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}