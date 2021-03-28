using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public partial struct Type
    {
        // Predefined Type
        public static Type Void = new Type(TypeDeclId.Void);
        public static Type Bool = new Type(TypeDeclId.Bool);
        public static Type Int = new Type(TypeDeclId.Int);
        public static Type String = new Type(TypeDeclId.String);
        public static Type Enumerable(Type itemType)
        {
            var typeContext = new TypeContextBuilder().Add(0, 0, itemType).Build();
            return new Type(TypeDeclId.Enumerable, typeContext);
        }
        public static Type Lambda = new Type(TypeDeclId.Lambda);
        public static Type List(Type itemType)
        {
            var typeContext = new TypeContextBuilder().Add(0, 0, itemType).Build();
            return new Type(TypeDeclId.List, typeContext);
        }

        public TypeDeclId DeclId { get; }
        public TypeContext TypeContext { get; }

        public Type(TypeDeclId declId)
        {
            DeclId = declId;
            TypeContext = Gum.IR0.TypeContext.Empty;
        }

        public Type(TypeDeclId declId, TypeContext typeContext)
        {
            DeclId = declId;
            TypeContext = typeContext;
        }        
    }
}
