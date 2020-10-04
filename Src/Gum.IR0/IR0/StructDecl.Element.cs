using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.IR0
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

            public override bool Equals(object? obj)
            {
                return obj is VarDeclElement element &&
                       AccessModifier == element.AccessModifier &&
                       EqualityComparer<TypeExp>.Default.Equals(VarType, element.VarType) &&
                       SeqEqComparer.Equals(VarNames, element.VarNames);
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(AccessModifier);
                hashCode.Add(VarType);
                SeqEqComparer.AddHash(ref hashCode, VarNames);

                return hashCode.ToHashCode();
            }

            public static bool operator ==(VarDeclElement? left, VarDeclElement? right)
            {
                return EqualityComparer<VarDeclElement?>.Default.Equals(left, right);
            }

            public static bool operator !=(VarDeclElement? left, VarDeclElement? right)
            {
                return !(left == right);
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

            public override bool Equals(object? obj)
            {
                return obj is FuncDeclElement element &&
                       AccessModifier == element.AccessModifier &&
                       IsStatic == element.IsStatic &&
                       IsSequence == element.IsSequence &&
                       EqualityComparer<TypeExp>.Default.Equals(RetType, element.RetType) &&
                       Name == element.Name &&
                       SeqEqComparer.Equals(TypeParams, element.TypeParams) &&
                       EqualityComparer<FuncParamInfo>.Default.Equals(ParamInfo, element.ParamInfo) &&
                       EqualityComparer<BlockStmt>.Default.Equals(Body, element.Body);
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(AccessModifier);
                hashCode.Add(IsStatic);
                hashCode.Add(IsSequence);
                hashCode.Add(RetType);
                hashCode.Add(Name);
                SeqEqComparer.AddHash(ref hashCode, TypeParams);
                hashCode.Add(ParamInfo);
                hashCode.Add(Body);
                return hashCode.ToHashCode();
            }

            public static bool operator ==(FuncDeclElement? left, FuncDeclElement? right)
            {
                return EqualityComparer<FuncDeclElement?>.Default.Equals(left, right);
            }

            public static bool operator !=(FuncDeclElement? left, FuncDeclElement? right)
            {
                return !(left == right);
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
