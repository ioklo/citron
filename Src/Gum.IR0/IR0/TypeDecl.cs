using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    public abstract partial class TypeDecl
    {
        public TypeDeclId Id { get; }

        public TypeDecl(TypeDeclId id)
        { 
            Id = id; 
        }
    }
}
