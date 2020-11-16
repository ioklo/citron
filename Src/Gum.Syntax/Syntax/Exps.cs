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
        public IdentifierExp(string value, params TypeExp[] typeArgs) { Value = value; TypeArgs = ImmutableArray.Create(typeArgs); }
    }

    public class StringExp : Exp
    {
        public ImmutableArray<StringExpElement> Elements { get; }
        
        public StringExp(IEnumerable<StringExpElement> elements)
        {
            Elements = elements.ToImmutableArray();
        }

        public StringExp(params StringExpElement[] elements)
        {
            Elements = ImmutableArray.Create(elements);
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

        public ImmutableArray<TypeExp> TypeArgs { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }

        public CallExp(Exp callable, IEnumerable<TypeExp> typeArgs, IEnumerable<Exp> args)
        {
            Callable = callable;
            TypeArgs = typeArgs.ToImmutableArray();
            Args = args.ToImmutableArray();
        }

        public CallExp(Exp callable, IEnumerable<TypeExp> typeArgs, params Exp[] args)
        {
            Callable = callable;
            TypeArgs = typeArgs.ToImmutableArray();
            Args = ImmutableArray.Create(args);
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

        public LambdaExp(IEnumerable<LambdaExpParam> parameters, Stmt body)
        {
            Params = parameters.ToImmutableArray();
            Body = body;
        }

        public LambdaExp(Stmt body, params LambdaExpParam[] parameters)
        {
            Params = ImmutableArray.Create(parameters);
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

    public class MemberCallExp : Exp
    {
        public Exp Object { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> MemberTypeArgs { get; }
        public ImmutableArray<Exp> Args { get; }

        public MemberCallExp(Exp obj, string memberName, IEnumerable<TypeExp> typeArgs, IEnumerable<Exp> args)
        {
            Object = obj;
            MemberName = memberName;
            MemberTypeArgs = typeArgs.ToImmutableArray();
            Args = args.ToImmutableArray();
        }

        public MemberCallExp(Exp obj, string memberName, IEnumerable<TypeExp> typeArgs, params Exp[] args)
        {
            Object = obj;
            MemberName = memberName;
            MemberTypeArgs = typeArgs.ToImmutableArray();
            Args = ImmutableArray.Create(args);
        }
    }

    public class MemberExp : Exp
    {
        public Exp Parent { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> MemberTypeArgs { get; }

        public MemberExp(Exp parent, string memberName, IEnumerable<TypeExp> memberTypeArgs)
        {
            Parent = parent;
            MemberName = memberName;
            MemberTypeArgs = memberTypeArgs.ToImmutableArray();
        }
    }

    public class ListExp : Exp
    {
        public TypeExp? ElemType { get; }
        public ImmutableArray<Exp> Elems { get; }

        public ListExp(TypeExp? elemType, IEnumerable<Exp> elems)
        {
            ElemType = elemType;
            Elems = elems.ToImmutableArray();
        }

        public ListExp(TypeExp? elemType, params Exp[] elems)
        {
            ElemType = elemType;
            Elems = ImmutableArray.Create(elems);
        }
    }

    // new Type(2, 3, 4);
    public class NewExp : Exp
    {
        public TypeExp Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }
        
        public NewExp(TypeExp type, IEnumerable<Exp> args)
        {
            Type = type;
            Args = args.ToImmutableArray();
        }
    }
}
