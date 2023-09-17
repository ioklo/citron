using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Citron.Syntax;

public abstract record class Exp : ISyntaxNode
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IExpVisitor<TResult>;
}

// 
public record class IdentifierExp(string Value, ImmutableArray<TypeExp> TypeArgs) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIdentifier(this);
    
}

public record class StringExp(ImmutableArray<StringExpElement> Elements) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitString(this);
}

public record class IntLiteralExp(int Value) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIntLiteral(this);
}

public record class BoolLiteralExp(bool Value) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoolLiteral(this);
}

// null
public record class NullLiteralExp : Exp
{
    public static readonly NullLiteralExp Instance = new NullLiteralExp();
    NullLiteralExp() { }

    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNullLiteral(this);
}

public record class BinaryOpExp(BinaryOpKind Kind, Exp Operand0, Exp Operand1) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBinaryOp(this);
}

public record class UnaryOpExp(UnaryOpKind Kind, Exp Operand) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitUnaryOp(this);
}

public record class CallExp(Exp Callable, ImmutableArray<Argument> Args) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCall(this);
}

public partial record struct LambdaExpParam(TypeExp? Type, string Name, bool HasParams);

public record class LambdaExp(ImmutableArray<LambdaExpParam> Params, ImmutableArray<Stmt> Body) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambda(this);
}

// a[b]
public record class IndexerExp(Exp Object, Exp Index) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIndexer(this);
}

public record class MemberExp(Exp Parent, string MemberName, ImmutableArray<TypeExp> MemberTypeArgs) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitMember(this);
}

public record class ListExp(ImmutableArray<Exp> Elems) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitList(this);
}

// new Type(2, 3, 4);
public record class NewExp(TypeExp Type, ImmutableArray<Argument> Args) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNew(this);
}

// box i
public record class BoxExp(Exp InnerExp) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBox(this);
}

// x is T
public record class IsExp(Exp Exp, TypeExp Type) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIs(this);
}

// x as T
public record class AsExp(Exp Exp, TypeExp Type) : Exp
{
    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitAs(this);
}

public interface IExpVisitor<TResult>
{
    TResult VisitIdentifier(IdentifierExp exp);
    TResult VisitString(StringExp exp);
    TResult VisitIntLiteral(IntLiteralExp exp);
    TResult VisitBoolLiteral(BoolLiteralExp exp);    
    TResult VisitNullLiteral(NullLiteralExp exp);
    TResult VisitBinaryOp(BinaryOpExp exp);
    TResult VisitUnaryOp(UnaryOpExp exp);
    TResult VisitCall(CallExp exp);
    TResult VisitLambda(LambdaExp exp);
    TResult VisitIndexer(IndexerExp exp);
    TResult VisitMember(MemberExp exp);
    TResult VisitList(ListExp exp);
    TResult VisitNew(NewExp exp);
    TResult VisitBox(BoxExp exp);
    TResult VisitIs(IsExp exp);
    TResult VisitAs(AsExp exp);
}