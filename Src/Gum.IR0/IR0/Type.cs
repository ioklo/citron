using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    public struct Type
    {
        // Predefined Type
        public static Type Void = new Type(TypeDeclId.Void);
        public static Type Bool = new Type(TypeDeclId.Bool);
        public static Type Int = new Type(TypeDeclId.Int);
        public static Type String = new Type(TypeDeclId.String);
        public static Type Enumerable(params Type[] typeArgs) => new Type(TypeDeclId.Enumerable, typeArgs);
        public static Type Lambda = new Type(TypeDeclId.Lambda);
        public static Type List(params Type[] typeArgs) => new Type(TypeDeclId.List, typeArgs);

        public TypeDeclId DeclId { get; }        
        public IReadOnlyList<Type> TypeArgs { get; }

        public Type(TypeDeclId declId)
        {
            DeclId = declId;
            TypeArgs = Array.Empty<Type>();
        }

        public Type(TypeDeclId declId, IEnumerable<Type> typeArgs)
        {
            DeclId = declId;
            TypeArgs = typeArgs.ToArray();
        }        
    }
}
