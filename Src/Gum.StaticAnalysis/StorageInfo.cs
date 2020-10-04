using Gum.CompileTime;
using Gum.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Gum
{
#pragma warning disable CS0660, CS0661
    public abstract class StorageInfo
    {
        public class ModuleGlobal : StorageInfo
        {
            public ModuleItemId VarId { get; }
            internal ModuleGlobal(ModuleItemId varId) { VarId = varId; }

            public override bool Equals(object? obj)
            {
                return obj is ModuleGlobal global &&
                       VarId.Equals(global.VarId);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(VarId);
            }
        }

        public class PrivateGlobal : StorageInfo
        {
            public int Index { get; }
            public PrivateGlobal(int index) { Index = index; }

            public override bool Equals(object? obj)
            {
                return obj is PrivateGlobal global &&
                       Index == global.Index;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index);
            }
        }

        public class Local : StorageInfo
        {
            public int Index { get; }
            public Local(int localIndex) { Index = localIndex; }

            public override bool Equals(object? obj)
            {
                return obj is Local local &&
                       Index == local.Index;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index);
            }
        }

        public class EnumElem : StorageInfo
        {
            public string Name { get; }
            public EnumElem(string name) { Name = name; }

            public override bool Equals(object? obj)
            {
                return obj is EnumElem elem &&
                       Name == elem.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name);
            }
        }

        public class StaticMember : StorageInfo
        {
            public (TypeValue TypeValue, Exp Exp)? ObjectInfo { get; }
            public VarValue VarValue { get; }
            public StaticMember((TypeValue TypeValue, Exp Exp)? objectInfo, VarValue varValue) { ObjectInfo = objectInfo; VarValue = varValue; }

            public override bool Equals(object? obj)
            {
                return obj is StaticMember member &&
                       EqualityComparer<(TypeValue TypeValue, Exp Exp)?>.Default.Equals(ObjectInfo, member.ObjectInfo) &&
                       EqualityComparer<VarValue>.Default.Equals(VarValue, member.VarValue);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ObjectInfo, VarValue);
            }
        }

        public class InstanceMember : StorageInfo
        {
            public Exp ObjectExp { get; }
            public TypeValue ObjectTypeValue { get; }
            public Name VarName { get; }
            public InstanceMember(Exp objectExp, TypeValue objectTypeValue, Name varName)
            {
                ObjectExp = objectExp;
                ObjectTypeValue = objectTypeValue;
                VarName = varName;
            }

            public override bool Equals(object? obj)
            {
                return obj is InstanceMember member &&
                       EqualityComparer<Exp>.Default.Equals(ObjectExp, member.ObjectExp) &&
                       EqualityComparer<TypeValue>.Default.Equals(ObjectTypeValue, member.ObjectTypeValue) &&
                       EqualityComparer<Name>.Default.Equals(VarName, member.VarName);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ObjectExp, ObjectTypeValue, VarName);
            }
        }

        public static ModuleGlobal MakeModuleGlobal(ModuleItemId varId) 
            => new ModuleGlobal(varId);

        public static PrivateGlobal MakePrivateGlobal(int index) 
            => new PrivateGlobal(index);

        public static Local MakeLocal(int index) 
            => new Local(index);

        public static EnumElem MakeEnumElem(string elemName)
            => new EnumElem(elemName);

        public static StaticMember MakeStaticMember((TypeValue TypeValue, Exp Exp)? objetInfo, VarValue varValue)
            => new StaticMember(objetInfo, varValue);

        public static InstanceMember MakeInstanceMember(Exp objectExp, TypeValue objectTypeValue, Name varName)
            => new InstanceMember(objectExp, objectTypeValue, varName);
        
        public static bool operator ==(StorageInfo? left, StorageInfo? right)
        {
            return EqualityComparer<StorageInfo?>.Default.Equals(left, right);
        }

        public static bool operator !=(StorageInfo? left, StorageInfo? right)
        {
            return !(left == right);
        }
    }
#pragma warning restore CS0660, CS0661
}
