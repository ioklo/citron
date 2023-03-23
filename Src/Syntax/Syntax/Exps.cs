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
    public abstract void Accept<TVisitor>(ref TVisitor visitor)
        where TVisitor : struct, IExpVisitor;
}

// 
public record class IdentifierExp(string Value, ImmutableArray<TypeExp> TypeArgs) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIdentifier(this);
    
}

public record class StringExp(ImmutableArray<StringExpElement> Elements) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitString(this);
}

public record class IntLiteralExp(int Value) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIntLiteral(this);
}

public record class BoolLiteralExp(bool Value) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBoolLiteral(this);
}

// null
public record class NullLiteralExp : Exp
{
    public static readonly NullLiteralExp Instance = new NullLiteralExp();
    NullLiteralExp() { }

    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitNullLiteral(this);
}

public record class BinaryOpExp(BinaryOpKind Kind, Exp Operand0, Exp Operand1) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBinaryOp(this);
}

public record class UnaryOpExp(UnaryOpKind Kind, Exp Operand) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitUnaryOp(this);
}

public record class CallExp(Exp Callable, ImmutableArray<Argument> Args) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitCall(this);
}

public partial record struct LambdaExpParam(FuncParamKind ParamKind, TypeExp? Type, string Name);

public record class LambdaExp(ImmutableArray<LambdaExpParam> Params, ImmutableArray<Stmt> Body) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitLambda(this);
}

// a[b]
public record class IndexerExp(Exp Object, Exp Index) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIndexer(this);
}

public record class MemberExp(Exp Parent, string MemberName, ImmutableArray<TypeExp> MemberTypeArgs) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitMember(this);
}

public record class ListExp(TypeExp? ElemType, ImmutableArray<Exp> Elems) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitList(this);
}

// new Type(2, 3, 4);
public record class NewExp(TypeExp Type, ImmutableArray<Argument> Args) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitNew(this);
}

// &i -> LocalRef, BoxRef로 분화된다
public record class RefExp(Exp InnerExp) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitRef(this);
}

// box i
public record class BoxExp(Exp InnerExp) : Exp
{
    public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBox(this);
}

public interface IExpVisitor
{
    void VisitIdentifier(IdentifierExp exp);
    void VisitString(StringExp exp);
    void VisitIntLiteral(IntLiteralExp exp);
    void VisitBoolLiteral(BoolLiteralExp exp);    
    void VisitNullLiteral(NullLiteralExp exp);
    void VisitBinaryOp(BinaryOpExp exp);
    void VisitUnaryOp(UnaryOpExp exp);
    void VisitCall(CallExp exp);
    void VisitLambda(LambdaExp exp);
    void VisitIndexer(IndexerExp exp);
    void VisitMember(MemberExp exp);
    void VisitList(ListExp exp);
    void VisitNew(NewExp exp);
    void VisitRef(RefExp exp);
    void VisitBox(BoxExp exp);
}