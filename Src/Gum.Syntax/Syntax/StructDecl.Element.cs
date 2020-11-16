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
                IEnumerable<string> varNames)
            {
                AccessModifier = accessModifier;
                VarType = varType;
                VarNames = varNames.ToImmutableArray();
            }
        }

        public class FuncDeclElement : Element
        {
            public AccessModifier AccessModifier { get; }
            public bool IsStatic { get; }
            public bool IsSequence { get; }
            public TypeExp RetType { get; }
            public string Name { get; }
            public ImmutableArray<string> TypeParams { get; }
            public FuncParamInfo ParamInfo { get; }
            public BlockStmt Body { get; }

            public FuncDeclElement(
                AccessModifier accessModifier,
                bool bStatic,
                bool bSequence,
                TypeExp retType,
                string name,
                IEnumerable<string> typeParams,
                FuncParamInfo paramInfo,
                BlockStmt body)
            {
                AccessModifier = accessModifier;
                IsStatic = bStatic;
                IsSequence = bSequence;                
                RetType = retType;
                Name = name;
                TypeParams = typeParams.ToImmutableArray();
                ParamInfo = paramInfo;
                Body = body;
            }
        }        
    }
}
