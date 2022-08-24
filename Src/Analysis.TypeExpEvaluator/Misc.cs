using Citron.Infra;
using Citron.Symbol;
using Citron.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{
    static class Misc
    {
        public static TypeExpInfoKind GetTypeExpInfoKind(this SkeletonKind kind)
        {
            switch (kind)
            {
                case SkeletonKind.Class: return TypeExpInfoKind.Class;
                case SkeletonKind.Struct: return TypeExpInfoKind.Struct;
                case SkeletonKind.Interface: return TypeExpInfoKind.Interface;
                case SkeletonKind.Enum: return TypeExpInfoKind.Enum;
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
