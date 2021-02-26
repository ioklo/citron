using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gum.Syntax
{
    public abstract class Exp : ISyntaxNode
    {
    }
    
    public class IdentifierExp : Exp
    {
        public string Value { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
        public IdentifierExp(string value, ImmutableArray<TypeExp> typeArgs) { Value = value; TypeArgs = typeArgs; }
    }

    public class StringExp : Exp
    {
        public ImmutableArray<StringExpElement> Elements { get; }
        
        public StringExp(ImmutableArray<StringExpElement> elements)
        {
            Elements = elements;
        }
    }

    public class IntLiteralExp : Exp
    {
        public int Value { get; }
        public IntLiteralExp(int value) { Value = value; }
    }

    public class BoolLiteralExp : Exp
    {
        public bool Value { get; }
        public BoolLiteralExp(bool value) { Value = value; }
    }

    public class BinaryOpExp : Exp
    {
        public BinaryOpKind Kind { get; }
        public Exp Operand0 { get; }
        public Exp Operand1 { get; }
        
        public BinaryOpExp(BinaryOpKind kind, Exp operand0, Exp operand1)
        {
            Kind = kind;
            Operand0 = operand0;
            Operand1 = operand1;
        }
    }

    public class UnaryOpExp : Exp
    {
        public UnaryOpKind Kind { get; }
        public Exp Operand{ get; }
        public UnaryOpExp(UnaryOpKind kind, Exp operand)
        {
            Kind = kind;
            Operand = operand;
        }
    }

    // MemberCallExp는 따로 
    public class CallExp : Exp
    {
        public Exp Callable { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }

        public CallExp(Exp callable, ImmutableArray<Exp> args)
        {
            Callable = callable;
            Args = args;
        }        
    }

    public struct LambdaExpParam
    {
        public TypeExp? Type { get; }
        public string Name { get; }

        public LambdaExpParam(TypeExp? type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class LambdaExp : Exp
    {
        public ImmutableArray<LambdaExpParam> Params { get; }
        public Stmt Body { get; }

        public LambdaExp(ImmutableArray<LambdaExpParam> parameters, Stmt body)
        {
            Params = parameters;
            Body = body;
        }        
    }
    
    // a[b]
    public class IndexerExp : Exp
    {
        public Exp Object { get; }
        public Exp Index { get; }

        public IndexerExp(Exp obj, Exp index)
        {
            Object = obj;
            Index = index;
        }
    }
    
    public class MemberExp : Exp
    {
        public Exp Parent { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> MemberTypeArgs { get; }

        public MemberExp(Exp parent, string memberName, ImmutableArray<TypeExp> memberTypeArgs)
        {
            Parent = parent;
            MemberName = memberName;
            MemberTypeArgs = memberTypeArgs;
        }
    }

    public class ListExp : Exp
    {
        public TypeExp? ElemType { get; }
        public ImmutableArray<Exp> Elems { get; }

        public ListExp(TypeExp? elemType, ImmutableArray<Exp> elems)
        {
            ElemType = elemType;
            Elems = elems;
        }        
    }

    // new Type(2, 3, 4);
    public class NewExp : Exp
    {
        public TypeExp Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }
        
        public NewExp(TypeExp type, ImmutableArray<Exp> args)
        {
            Type = type;
            Args = args;
        }
    }
}
