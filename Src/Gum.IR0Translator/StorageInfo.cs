using Gum.CompileTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using S = Gum.Syntax;

namespace Gum
{
#pragma warning disable CS0660, CS0661
    abstract class StorageInfo
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
            public string Name { get; }
            public PrivateGlobal(string name) { Name = name; }

            public override bool Equals(object? obj)
            {
                return obj is PrivateGlobal global &&
                       Name == global.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name);
            }
        }

        public class Local : StorageInfo
        {
            public string Name { get; }
            public Local(string name) { Name = name; }

            public override bool Equals(object? obj)
            {
                return obj is Local local &&
                       Name == local.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name);
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
            public (TypeValue TypeValue, S.Exp Exp)? ObjectInfo { get; }
            public VarValue VarValue { get; }
            public StaticMember((TypeValue TypeValue, S.Exp Exp)? objectInfo, VarValue varValue) { ObjectInfo = objectInfo; VarValue = varValue; }

            public override bool Equals(object? obj)
            {
                return obj is StaticMember member &&
                       EqualityComparer<(TypeValue TypeValue, S.Exp Exp)?>.Default.Equals(ObjectInfo, member.ObjectInfo) &&
                       EqualityComparer<VarValue>.Default.Equals(VarValue, member.VarValue);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ObjectInfo, VarValue);
            }
        }

        public class InstanceMember : StorageInfo
        {
            public S.Exp ObjectExp { get; }
            public TypeValue ObjectTypeValue { get; }
            public Name VarName { get; }
            public InstanceMember(S.Exp objectExp, TypeValue objectTypeValue, Name varName)
            {
                ObjectExp = objectExp;
                ObjectTypeValue = objectTypeValue;
                VarName = varName;
            }

            public override bool Equals(object? obj)
            {
                return obj is InstanceMember member &&
                       EqualityComparer<S.Exp>.Default.Equals(ObjectExp, member.ObjectExp) &&
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

        public static PrivateGlobal MakePrivateGlobal(string name) 
            => new PrivateGlobal(name);

        public static Local MakeLocal(string name) 
            => new Local(name);

        public static EnumElem MakeEnumElem(string elemName)
            => new EnumElem(elemName);

        public static StaticMember MakeStaticMember((TypeValue TypeValue, S.Exp Exp)? objetInfo, VarValue varValue)
            => new StaticMember(objetInfo, varValue);

        public static InstanceMember MakeInstanceMember(S.Exp objectExp, TypeValue objectTypeValue, Name varName)
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
