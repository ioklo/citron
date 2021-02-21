using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public abstract class StructDeclElement : ISyntaxNode
    {
        // 외부에서 상속 금지
        internal StructDeclElement() { }
    }    

    public class TypeStructDeclElement : StructDeclElement
    {
        public TypeDecl TypeDecl { get; }
        public TypeStructDeclElement(TypeDecl typeDecl)
        {
            TypeDecl = typeDecl;
        }
    }

    public class VarStructDeclElement : StructDeclElement
    {
        public AccessModifier AccessModifier { get; }
        public TypeExp VarType { get; }
        public ImmutableArray<string> VarNames { get; }

        public VarStructDeclElement(
            AccessModifier accessModifier,
            TypeExp varType,
            ImmutableArray<string> varNames)
        {
            AccessModifier = accessModifier;
            VarType = varType;
            VarNames = varNames;
        }
    }

    public class FuncStructDeclElement : StructDeclElement
    {
        public StructFuncDecl FuncDecl { get; }

        public FuncStructDeclElement(StructFuncDecl funcDecl)
        {
            FuncDecl = funcDecl;
        }
    }
}
