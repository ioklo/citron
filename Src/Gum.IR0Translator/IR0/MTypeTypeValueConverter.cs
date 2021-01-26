using Gum.CompileTime;
using System.Collections.Immutable;
using System.Linq;


using Gum.Misc;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    struct MTypeTypeValueConverter
    {
        ModuleName internalModuleName;

        public MTypeTypeValueConverter(ModuleName internalModuleName)
        {
            this.internalModuleName = internalModuleName;
        }

        public static TypeValue ToTypeValue(ModuleName internalModuleName, M.Type mtype)
        {
            MTypeTypeValueConverter converter = new MTypeTypeValueConverter(internalModuleName);
            return converter.ToTypeValue(mtype);
        }

        ImmutableArray<TypeValue> ToTypeValues(ImmutableArray<M.Type> mtypes)
        {
            var builder = ImmutableArray.CreateBuilder<TypeValue>();
            foreach (var mtype in mtypes)
            {
                var type = ToTypeValue(mtype);
                builder.Add(type);
            }

            return builder.ToImmutable();
        }

        TypeValue ToTypeValue(M.Type mtype)
        {
            switch (mtype)
            {
                case TypeVarType typeVar:
                    return new TypeValue.TypeVar(typeVar.Depth, typeVar.Index, typeVar.Name);

                case InternalType internalType:
                    {
                        var typeArgs = ToTypeValues(internalType.TypeArgs);
                        var entry = new AppliedItemPathEntry(internalType.Name, string.Empty, typeArgs);
                        var path = new AppliedItemPath(internalType.NamespacePath, entry);

                        return new TypeValue.Normal(internalModuleName, path);
                    }

                case ExternalType externalType:
                    {
                        var typeArgs = ToTypeValues(externalType.TypeArgs);
                        var entry = new AppliedItemPathEntry(externalType.Name, string.Empty);
                        var path = new AppliedItemPath(externalType.NamespacePath, entry);

                        return new TypeValue.Normal(externalType.ModuleName, path);
                    }

                case MemberType memberType:
                    {
                        var outerType = ToTypeValue(memberType.Outer) as TypeValue.Normal;
                        Debug.Assert(outerType != null);

                        var typeArgs = ToTypeValues(memberType.TypeArgs);
                        var entry = new AppliedItemPathEntry(memberType.Name, string.Empty, typeArgs);
                        var path = outerType.Path.Append(entry);

                        return new TypeValue.Normal(outerType.ModuleName, path);
                    }

                case VoidType _:
                    return TypeValue.Void.Instance;

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
