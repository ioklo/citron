using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Analysis
{
    static class Misc
    {
        public static TypeExpInfoKind GetTypeExpInfoKind(this TypeSkeletonKind kind)
        {
            switch (kind)
            {
                case TypeSkeletonKind.Class: return TypeExpInfoKind.Class;
                case TypeSkeletonKind.Struct: return TypeExpInfoKind.Struct;
                case TypeSkeletonKind.Interface: return TypeExpInfoKind.Interface;
                case TypeSkeletonKind.Enum: return TypeExpInfoKind.Enum;
            }

            throw new UnreachableCodeException();
        }

        public static TypeExpInfoKind GetTypeExpInfoKind(this ITypeDeclSymbol node)
        {
            return node switch
            {
                ClassDeclSymbol => TypeExpInfoKind.Class,
                StructDeclSymbol => TypeExpInfoKind.Struct,
                EnumDeclSymbol => TypeExpInfoKind.Enum,
                EnumElemDeclSymbol => TypeExpInfoKind.EnumElem,
                _ => throw new UnreachableCodeException()
            };
        }
    }
}
