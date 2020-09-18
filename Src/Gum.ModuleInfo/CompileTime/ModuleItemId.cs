using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    // (System.Runtime, System.X<,>.Y<,,>.T)
    // Absolute, Relative 둘다
    public class ModuleItemId
    {
        public ModuleItemId? Outer { get; }
        private ModuleItemIdElem elem;

        public Name Name { get => elem.Name; }
        public int TypeParamCount { get => elem.TypeParamCount; }

        public static ModuleItemId Make(ModuleItemIdElem elem0, params ModuleItemIdElem[] elems)
        {
            var curId = new ModuleItemId(null, elem0);

            foreach (var elem in elems)
                curId = new ModuleItemId(curId, elem);

            return curId;
        }

        public static ModuleItemId Make(string name, int typeParamCount = 0)
        {
            return new ModuleItemId(null, new ModuleItemIdElem(Name.MakeText(name), typeParamCount));
        }

        public static ModuleItemId Make(Name name, int typeParamCount = 0)
        {
            return new ModuleItemId(null, new ModuleItemIdElem(name, typeParamCount));
        }
        
        private ModuleItemId(ModuleItemId? outer, ModuleItemIdElem elem)
        {
            Outer = outer;
            this.elem = elem;
        }

        public static bool operator ==(ModuleItemId? left, ModuleItemId? right)
        {
            return EqualityComparer<ModuleItemId?>.Default.Equals(left, right);
        }

        public static bool operator !=(ModuleItemId? left, ModuleItemId? right)
        {
            return !(left == right);
        }

        public void ToString(StringBuilder sb)
        {
            if (Outer != null)
            {
                Outer.ToString(sb);
                sb.Append(".");
            }

            elem.ToString(sb);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public ModuleItemId Append(ModuleItemIdElem elem)
        {
            return new ModuleItemId(this, elem);
        }
        
        public ModuleItemId Append(string name, int typeParamCount = 0)
        {
            return new ModuleItemId(this, new ModuleItemIdElem(name, typeParamCount));
        }

        public ModuleItemId Append(Name name, int typeParamCount = 0)
        {
            return new ModuleItemId(this, new ModuleItemIdElem(name, typeParamCount));
        }

        public override bool Equals(object? obj)
        {
            return obj is ModuleItemId id &&
                   EqualityComparer<ModuleItemId?>.Default.Equals(Outer, id.Outer) &&
                   EqualityComparer<ModuleItemIdElem>.Default.Equals(elem, id.elem);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Outer, elem);
        }
    }
}