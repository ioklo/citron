using Citron.Collections;

namespace Citron.Syntax
{
    public abstract record ClassMemberDecl : ISyntaxNode;    

    public record ClassMemberTypeDecl(TypeDecl TypeDecl) : ClassMemberDecl;

    public record ClassMemberFuncDecl(
        AccessModifier? AccessModifier,
        bool IsStatic,
        bool IsSequence, // seq 함수인가
        TypeExp RetType,
        string Name,
        ImmutableArray<TypeParam> TypeParams,
        ImmutableArray<FuncParam> Parameters,
        ImmutableArray<Stmt> Body
    ) : ClassMemberDecl;
    
    public record ClassConstructorDecl(
        AccessModifier? AccessModifier, 
        string Name, 
        ImmutableArray<FuncParam> Parameters, 
        ImmutableArray<Argument>? BaseArgs,
        ImmutableArray<Stmt> Body) : ClassMemberDecl;

    public record ClassMemberVarDecl(AccessModifier? AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : ClassMemberDecl;
}