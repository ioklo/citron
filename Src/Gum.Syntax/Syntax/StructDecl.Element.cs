using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public partial class StructDecl : TypeDecl
    {
        public abstract class Element : ISyntaxNode
        {
            // 외부에서 상속 금지
            internal Element() { }
        }

        public class TypeDeclElement : Element
        {
            public TypeDecl TypeDecl { get; }
            public TypeDeclElement(TypeDecl typeDecl)
            {
                TypeDecl = typeDecl;
            }
        }

        public class VarDeclElement : Element
        {
            public AccessModifier AccessModifier { get; }
            public TypeExp VarType { get; }
            public ImmutableArray<string> VarNames { get; }

            public VarDeclElement(
                AccessModifier accessModifier,             
                TypeExp varType,
                ImmutableArray<string> varNames)
            {
                AccessModifier = accessModifier;
                VarType = varType;
                VarNames = varNames;
            }
        }        

        public class FuncDeclElement : Element
        {
            public StructFuncDecl FuncDecl { get; }

            public FuncDeclElement(StructFuncDecl funcDecl)
            {
                FuncDecl = funcDecl;
            }
        }        
    }
}
