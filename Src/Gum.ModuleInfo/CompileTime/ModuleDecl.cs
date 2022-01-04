using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Gum.Infra;
using System.Diagnostics.Contracts;

namespace Gum.CompileTime
{
    public record ModuleDecl(Name Name, ImmutableArray<NamespaceDecl> Namespaces, ImmutableArray<GlobalTypeDecl> Types, ImmutableArray<GlobalFuncDecl> Funcs);
    public record NamespaceDecl(Name Name, ImmutableArray<NamespaceDecl> Namespaces, ImmutableArray<GlobalTypeDecl> Types, ImmutableArray<GlobalFuncDecl> Funcs);

    // 
    public abstract record TypeDecl;

    // Global
    public record GlobalTypeDecl(AccessModifier AccessModifier, TypeDecl TypeDecl);
    public record GlobalFuncDecl(AccessModifier AccessModifier, Name Name, bool IsRefReturn, TypeId RetType, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters);

    // Class     
    public record ClassDecl(Name Name, ImmutableArray<string> TypeParams, TypeId? BaseType, ImmutableArray<TypeId> Interfaces, ImmutableArray<ClassMemberTypeDecl> MemberTypes, ImmutableArray<ClassConstructorDecl> Constructors, ImmutableArray<ClassMemberFuncDecl> MemberFuncs, ImmutableArray<ClassMemberVarDecl> MemberVars) : TypeDecl;
    public record ClassMemberTypeDecl(AccessModifier AccessModifier, TypeDecl TypeDecl);
    public record ClassConstructorDecl(AccessModifier AccessModifier, ImmutableArray<Param> Parameters, bool IsTrivial);
    public record ClassMemberFuncDecl(AccessModifier AccessModifier, Name Name, bool IsRefReturn, bool IsStatic, ImmutableArray<string> TypeParams, TypeId RetType, ImmutableArray<Param> Parameters);
    public record ClassMemberVarDecl(AccessModifier AccessModifier, bool IsStatic, TypeId Type, Name Name);

    // Struct
    public record StructDecl(Name Name, ImmutableArray<string> TypeParams, TypeId? BaseType, ImmutableArray<TypeId> Interfaces, ImmutableArray<StructMemberTypeDecl> MemberTypes, ImmutableArray<StructConstructorDecl> Constructors, ImmutableArray<StructMemberFuncDecl> MemberFuncs, ImmutableArray<StructMemberVarDecl> MemberVars) : TypeDecl;
    public record StructMemberTypeDecl(AccessModifier AccessModifier, TypeDecl TypeDecl);
    public record StructConstructorDecl(AccessModifier AccessModifier, ImmutableArray<Param> Parameters, bool IsTrivial);
    public record StructMemberFuncDecl(AccessModifier AccessModifier, Name Name, bool IsRefReturn, bool IsStatic, ImmutableArray<string> TypeParams, TypeId RetType, ImmutableArray<Param> Parameters);
    public record StructMemberVarDecl(AccessModifier AccessModifier, bool IsStatic, TypeId Type, Name Name);

    // Enum
    public record EnumDecl(Name Name, ImmutableArray<string> TypeParams, ImmutableArray<EnumElemDecl> ElemDecls) : TypeDecl;
    public record EnumElemDecl(Name Name, ImmutableArray<EnumElemMemberVarDecl> MemberVars); // Type이지만, ModuleDecl에서는 TypeDecl로 보지 않는다 (TypeDecl 이름이 문제다)
    public record EnumElemMemberVarDecl(TypeId DeclType, Name Name);
}
