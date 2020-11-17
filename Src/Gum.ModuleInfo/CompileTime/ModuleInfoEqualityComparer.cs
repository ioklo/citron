using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gum.CompileTime
{
    public class ModuleInfoEqualityComparer 
        : IEqualityComparer<AppliedItemPathEntry>         
        , IEqualityComparer<TypeValue>
        , IEqualityComparer<ItemPath>
        , IEqualityComparer<ItemPathEntry>
        , IEqualityComparer<NamespaceId>
        , IEqualityComparer<ItemInfo>
    {
        public static ModuleInfoEqualityComparer Instance { get; } = new ModuleInfoEqualityComparer();
        private ModuleInfoEqualityComparer() { }

        bool IEqualityComparer<AppliedItemPathEntry>.Equals(AppliedItemPathEntry x, AppliedItemPathEntry y) => EqualsAppliedItemPathEntry(x, y);
        int IEqualityComparer<AppliedItemPathEntry>.GetHashCode(AppliedItemPathEntry obj) => GetHashCodeAppliedItemPathEntry(obj);
        bool IEqualityComparer<TypeValue>.Equals(TypeValue x, TypeValue y) => EqualsTypeValue(x, y);
        int IEqualityComparer<TypeValue>.GetHashCode([DisallowNull] TypeValue? obj) => GetHashCodeTypeValue(obj);
        bool IEqualityComparer<ItemPath>.Equals(ItemPath x, ItemPath y) => EqualsItemPath(x, y);
        int IEqualityComparer<ItemPath>.GetHashCode(ItemPath obj) => GetHashCodeItemPath(obj);

        bool IEqualityComparer<ItemPathEntry>.Equals(ItemPathEntry x, ItemPathEntry y) => EqualsItemPathEntry(x, y);
        int IEqualityComparer<ItemPathEntry>.GetHashCode(ItemPathEntry obj) => GetHashCodeItemPathEntry(obj);

        bool IEqualityComparer<NamespaceId>.Equals(NamespaceId x, NamespaceId y) => EqualsNamespaceId(x, y);
        int IEqualityComparer<NamespaceId>.GetHashCode(NamespaceId obj) => GetHashCodeNamespaceId(obj);

        bool IEqualityComparer<ItemInfo>.Equals(ItemInfo x, ItemInfo y) => EqualsItemInfo(x, y);
        int IEqualityComparer<ItemInfo>.GetHashCode(ItemInfo obj) => GetHashCodeItemInfo(obj);

        static bool EqualsSequence<T>(ImmutableArray<T> x, ImmutableArray<T> y, IEqualityComparer<T> comparer)
        {
            return x.SequenceEqual(y, comparer);
        }

        static int GetHashCodeSequence<T>(ImmutableArray<T> array, IEqualityComparer<T> comparer)
        {
            var hashCode = new HashCode();

            foreach (var elem in array)
                hashCode.Add(comparer.GetHashCode(elem));

            return hashCode.ToHashCode();
        }

        public static bool EqualsModuleName(ModuleName x, ModuleName y)
        {
            return x.Kind == y.Kind && x.Text == y.Text;
        }

        public static int GetHashCodeModuleName(ModuleName obj)
        {
            return HashCode.Combine(obj.Kind, obj.Text);
        }

        public static bool EqualsNamespacePath(NamespacePath x, NamespacePath y)
        {
            return EqualsSequence(x.Entries, y.Entries, Instance);
        }

        public static int GetHashCodeNamespacePath(NamespacePath obj)
        {
            return HashCode.Combine(GetHashCodeSequence(obj.Entries, Instance));
        }

        public static bool EqualsAppliedItemPathEntry(AppliedItemPathEntry x, AppliedItemPathEntry y)
        {
            return EqualsName(x.Name, y.Name) &&
                x.ParamHash == y.ParamHash &&
                EqualsSequence(x.TypeArgs, y.TypeArgs, Instance);
        }

        public static int GetHashCodeAppliedItemPathEntry(AppliedItemPathEntry obj)
        {
            return HashCode.Combine(GetHashCodeName(obj.Name), obj.ParamHash, GetHashCodeSequence(obj.TypeArgs, Instance));
        }

        public static bool EqualsTypeValue(TypeValue? x, TypeValue? y)
        {
            switch ((x, y))
            {
                case (null, null):
                    return true;

                // singletons
                case (TypeValue.Var _, TypeValue.Var _):
                case (TypeValue.Void _, TypeValue.Void _):
                    return true;

                case (TypeValue.TypeVar typeVarX, TypeValue.TypeVar typeVarY):
                    return typeVarX.Depth == typeVarY.Depth &&
                        EqualsName(typeVarX.Name, typeVarY.Name);

                case (TypeValue.Normal normalX, TypeValue.Normal normalY):
                    return EqualsModuleName(normalX.ModuleName, normalY.ModuleName) &&
                        EqualsNamespacePath(normalX.NamespacePath, normalY.NamespacePath) &&
                        EqualsSequence(normalX.OuterEntries, normalY.OuterEntries, Instance) &&
                        EqualsAppliedItemPathEntry(normalX.Entry, normalY.Entry);

                case (TypeValue.Func funcX, TypeValue.Func funcY):
                    return EqualsSequence(funcX.Params, funcY.Params, Instance) &&
                        EqualsTypeValue(funcX.Return, funcY.Return);

                case (TypeValue.EnumElem enumElemX, TypeValue.EnumElem enumElemY):
                    return EqualsTypeValue(enumElemX.EnumTypeValue, enumElemY.EnumTypeValue) &&
                        enumElemX.Name == enumElemY.Name;

                default:
                    return false;
            }
        }

        public static int GetHashCodeTypeValue(TypeValue obj)
        {
            switch (obj)
            {
                case TypeValue.Var _:
                    return TypeValue.Var.Instance.GetHashCode();

                case TypeValue.Void _:
                    return TypeValue.Void.Instance.GetHashCode();

                case TypeValue.TypeVar typeVar:
                    return HashCode.Combine(
                        typeVar.Depth,                        
                        typeVar.Name);

                case TypeValue.Normal normal:
                    return HashCode.Combine(
                        GetHashCodeModuleName(normal.ModuleName),
                        GetHashCodeNamespacePath(normal.NamespacePath),
                        GetHashCodeSequence(normal.OuterEntries, Instance),
                        GetHashCodeAppliedItemPathEntry(normal.Entry));

                case TypeValue.Func func:
                    return HashCode.Combine(
                        GetHashCodeSequence(func.Params, Instance),
                        GetHashCodeTypeValue(func.Return));

                case TypeValue.EnumElem enumElem:
                    return HashCode.Combine(
                        GetHashCodeTypeValue(enumElem.EnumTypeValue),
                        GetHashCodeName(enumElem.Name));

                default:
                    throw new InvalidOperationException();
            }
        }

        public static bool EqualsItemId(ItemId x, ItemId y)
        {
            return EqualsModuleName(x.ModuleName, y.ModuleName) &&
                EqualsItemPath(x.path, y.path);
        }

        public static int GetHashCodeItemId(ItemId obj)
        {
            return HashCode.Combine(GetHashCodeModuleName(obj.ModuleName), GetHashCodeItemPath(obj.path));
        }

        // struct
        public static bool EqualsItemPath(ItemPath x, ItemPath y)
        {
            return EqualsNamespacePath(x.NamespacePath, y.NamespacePath) &&
                EqualsSequence(x.OuterEntries, y.OuterEntries, Instance) &&
                EqualsItemPathEntry(x.Entry, y.Entry); // Equatable
        }

        public static int GetHashCodeItemPath(ItemPath obj)
        {
            return HashCode.Combine(
                GetHashCodeNamespacePath(obj.NamespacePath),
                GetHashCodeSequence(obj.OuterEntries, Instance),
                GetHashCodeItemPathEntry(obj.Entry));
        }

        // struct
        public static bool EqualsItemPathEntry(ItemPathEntry x, ItemPathEntry y)
        {
            return EqualsName(x.Name, y.Name) && x.TypeParamCount == y.TypeParamCount && x.ParamHash == y.ParamHash;
        }

        public static int GetHashCodeItemPathEntry(ItemPathEntry obj)
        {
            return HashCode.Combine(
                GetHashCodeName(obj.Name),
                obj.TypeParamCount, // int
                obj.ParamHash);                
        }

        public static bool EqualsName(Name x, Name y)
        {
            return x.Kind == y.Kind && x.Text == y.Text;
        }

        public static int GetHashCodeName(Name obj)
        {
            return HashCode.Combine(obj.Kind, obj.Text);
        }

        public static bool EqualsNamespaceId(NamespaceId x, NamespaceId y)
        {
            return x.Value == y.Value;
        }

        public static int GetHashCodeNamespaceId(NamespaceId obj)
        {
            return obj.Value.GetHashCode();
        }

        public static bool EqualsItemInfo(ItemInfo? x, ItemInfo? y)
        {
            switch((x, y))
            {
                case (null, null): 
                    return true;

        //            public bool bSeqCall { get; }
        //public bool bThisCall { get; }
        //public ImmutableArray<string> TypeParams { get; }
        //public TypeValue RetTypeValue { get; }
        //public ImmutableArray<TypeValue> ParamTypeValues { get; }

                case (FuncInfo funcInfoX, FuncInfo funcInfoY):
                    return funcInfoX.bSeqCall == funcInfoY.bSeqCall &&
                        funcInfoX.bThisCall == funcInfoY.bThisCall &&
                        funcInfoX.TypeParams.SequenceEqual(funcInfoY.TypeParams) && // string
                        EqualsTypeValue(funcInfoX.RetTypeValue, funcInfoY.RetTypeValue) &&
                        EqualsSequence(funcInfoX.ParamTypeValues, funcInfoY.ParamTypeValues, Instance);

                default:
                    throw new NotImplementedException();
            }
        }

        public int GetHashCodeItemInfo(ItemInfo obj)
        {
            throw new NotImplementedException();
        }
    }
}