using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public partial class StructDecl
    {
        public abstract class Element
        {
            // 외부에서 상속 금지
            internal Element() { }
        }

        public class VarDeclElement : Element
        {
            public AccessModifier AccessModifier { get; }
            public TypeExp VarType { get; }
            public ImmutableArray<string> VarNames { get; }

            internal VarDeclElement(
                AccessModifier accessModifier,             
                TypeExp varType,
                IEnumerable<string> varNames)
            {
                AccessModifier = accessModifier;
                VarType = VarType;
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

            internal FuncDeclElement(
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

        public static VarDeclElement MakeVarDeclElement(AccessModifier accessModifier, TypeExp varType, IEnumerable<string> varNames)
            => new VarDeclElement(accessModifier, varType, varNames);

        public static FuncDeclElement MakeFuncDeclElement(
            AccessModifier accessModifier,
            bool bStatic,
            bool bSequence,
            TypeExp retType,
            string name,
            IEnumerable<string> typeParams,
            FuncParamInfo paramInfo,
            BlockStmt body)
            => new FuncDeclElement(accessModifier, bStatic, bSequence, retType, name, typeParams, paramInfo, body);
    }
}
