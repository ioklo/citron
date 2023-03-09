using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Citron.Syntax
{
    public abstract record class Exp : ISyntaxNode;

    // 
    public record class IdentifierExp(string Value, ImmutableArray<TypeExp> TypeArgs) : Exp;
    public record class StringExp(ImmutableArray<StringExpElement> Elements) : Exp;    
    public record class IntLiteralExp(int Value) : Exp;
    public record class BoolLiteralExp(bool Value) : Exp;
    // null
    public record class NullLiteralExp : Exp
    {
        public static readonly NullLiteralExp Instance = new NullLiteralExp();
        NullLiteralExp() { }
    }

    public record class BinaryOpExp(BinaryOpKind Kind, Exp Operand0, Exp Operand1) : Exp;
    public record class UnaryOpExp(UnaryOpKind Kind, Exp Operand) : Exp;
    public record CallExp(Exp Callable, ImmutableArray<Argument> Args) : Exp;
    
    public partial record struct LambdaExpParam(FuncParamKind ParamKind, TypeExp? Type, string Name);

    public record class LambdaExp(ImmutableArray<LambdaExpParam> Params, ImmutableArray<Stmt> Body) : Exp;
    
    // a[b]
    public record class IndexerExp(Exp Object, Exp Index) : Exp;    
    public record class MemberExp(Exp Parent, string MemberName, ImmutableArray<TypeExp> MemberTypeArgs) : Exp;
    public record class ListExp(TypeExp? ElemType, ImmutableArray<Exp> Elems) : Exp;

    // new Type(2, 3, 4);
    public record class NewExp(TypeExp Type, ImmutableArray<Argument> Args) : Exp;
}
