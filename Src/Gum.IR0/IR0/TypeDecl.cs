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

        public class TypeDeclTypeVariable : TypeDecl
        {
            public TypeDeclId ParentId { get; }
            public string Name { get; }

            public TypeDeclTypeVariable(TypeDeclId id, TypeDeclId parentId, string name)
                : base(id)
            {
                ParentId = parentId;
                Name = name;
            }
        }

        public class FuncDeclTypeVariable : TypeDecl
        {
            public FuncDeclId ParentId { get; }
            public string Name { get; }

            public FuncDeclTypeVariable(TypeDeclId id, FuncDeclId parentId, string name)
                : base(id)
            {
                ParentId = parentId;
                Name = name;
            }
        }
    }
}
