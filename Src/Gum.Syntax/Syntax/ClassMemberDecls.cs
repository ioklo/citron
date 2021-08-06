using Gum.Collections;

namespace Gum.Syntax
{
    public abstract record ClassMemberDecl : ISyntaxNode;    

    public record ClassMemberTypeDecl(TypeDecl TypeDecl) : ClassMemberDecl;

    public record ClassMemberFuncDecl(
        AccessModifier? AccessModifier,
        bool IsStatic,
        bool IsSequence, // seq 함수인가
        bool IsRefReturn,
        TypeExp RetType,
        string Name,
        ImmutableArray<string> TypeParams,
        ImmutableArray<FuncParam> Parameters,
        BlockStmt Body
    ) : ClassMemberDecl;

    public record ClassConstructorDecl(AccessModifier? AccessModifier, string Name, ImmutableArray<FuncParam> Parameters, BlockStmt Body) : ClassMemberDecl;

    public record ClassMemberVarDecl(AccessModifier? AccessModifier, TypeExp VarType, ImmutableArray<string> VarNames) : ClassMemberDecl;
}