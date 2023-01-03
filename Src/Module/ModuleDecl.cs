using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Citron.Infra;
using System.Diagnostics.Contracts;

namespace Citron.Module
{
    public record class ModuleDecl(Name Name, NamespaceDecl Root);
    public record class NamespaceDecl(Name Name, ImmutableArray<NamespaceDecl> Namespaces, ImmutableArray<GlobalTypeDecl> Types, ImmutableArray<GlobalFuncDecl> Funcs);

    // 
    public abstract record class TypeDecl;

    // Global
    public record class GlobalTypeDecl(Accessor AccessModifier, TypeDecl TypeDecl);
    public record class GlobalFuncDecl(Accessor AccessModifier, Name Name, bool IsRefReturn, TypeId RetType, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters);

    // Class     
    public record ClassDecl(Name Name, ImmutableArray<string> TypeParams, TypeId? BaseType, ImmutableArray<TypeId> Interfaces, ImmutableArray<ClassMemberTypeDecl> MemberTypes, ImmutableArray<ClassConstructorDecl> Constructors, ImmutableArray<ClassMemberFuncDecl> MemberFuncs, ImmutableArray<ClassMemberVarDecl> MemberVars) : TypeDecl;
    public record ClassMemberTypeDecl(Accessor AccessModifier, TypeDecl TypeDecl);
    public record ClassConstructorDecl(Accessor AccessModifier, ImmutableArray<Param> Parameters, bool IsTrivial);
    public record ClassMemberFuncDecl(Accessor AccessModifier, Name Name, bool IsRefReturn, bool IsStatic, ImmutableArray<string> TypeParams, TypeId RetType, ImmutableArray<Param> Parameters);
    public record ClassMemberVarDecl(Accessor AccessModifier, bool IsStatic, TypeId Type, Name Name);

    // Struct
    public record class StructDecl(Name Name, ImmutableArray<string> TypeParams, TypeId? BaseType, ImmutableArray<TypeId> Interfaces, ImmutableArray<StructMemberTypeDecl> MemberTypes, ImmutableArray<StructConstructorDecl> Constructors, ImmutableArray<StructMemberFuncDecl> MemberFuncs, ImmutableArray<StructMemberVarDecl> MemberVars) : TypeDecl;
    public record class StructMemberTypeDecl(Accessor AccessModifier, TypeDecl TypeDecl);
    public record class StructConstructorDecl(Accessor AccessModifier, ImmutableArray<Param> Parameters, bool IsTrivial);
    public record class StructMemberFuncDecl(Accessor AccessModifier, Name Name, bool IsRefReturn, bool IsStatic, ImmutableArray<string> TypeParams, TypeId RetType, ImmutableArray<Param> Parameters);
    public record class StructMemberVarDecl(Accessor AccessModifier, bool IsStatic, TypeId Type, Name Name);

    // Enum
    public record class EnumDecl(Name Name, ImmutableArray<string> TypeParams, ImmutableArray<EnumElemDecl> ElemDecls) : TypeDecl;
    public record class EnumElemDecl(Name Name, ImmutableArray<EnumElemMemberVarDecl> MemberVars); // Type이지만, ModuleDecl에서는 TypeDecl로 보지 않는다 (TypeDecl 이름이 문제다)
    public record class EnumElemMemberVarDecl(TypeId DeclType, Name Name);
}
