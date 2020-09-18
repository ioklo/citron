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

        public override bool Equals(object? obj)
        {
            return obj is IdentifierExp exp &&
                   Value == exp.Value &&
                   Enumerable.SequenceEqual(TypeArgs, exp.TypeArgs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(IdentifierExp? left, IdentifierExp? right)
        {
            return EqualityComparer<IdentifierExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(IdentifierExp? left, IdentifierExp? right)
        {
            return !(left == right);
        }
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

        public override bool Equals(object? obj)
        {
            return obj is StringExp exp && Enumerable.SequenceEqual(Elements, exp.Elements);                   
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            foreach (var elem in Elements)
                hashCode.Add(elem);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(StringExp? left, StringExp? right)
        {
            return EqualityComparer<StringExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(StringExp? left, StringExp? right)
        {
            return !(left == right);
        }
    }

    public class IntLiteralExp : Exp
    {
        public int Value { get; }
        public IntLiteralExp(int value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is IntLiteralExp exp &&
                   Value == exp.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(IntLiteralExp? left, IntLiteralExp? right)
        {
            return EqualityComparer<IntLiteralExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(IntLiteralExp? left, IntLiteralExp? right)
        {
            return !(left == right);
        }
    }

    public class BoolLiteralExp : Exp
    {
        public bool Value { get; }
        public BoolLiteralExp(bool value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is BoolLiteralExp exp &&
                   Value == exp.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(BoolLiteralExp? left, BoolLiteralExp? right)
        {
            return EqualityComparer<BoolLiteralExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(BoolLiteralExp? left, BoolLiteralExp? right)
        {
            return !(left == right);
        }
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

        public override bool Equals(object? obj)
        {
            return obj is BinaryOpExp exp &&
                   Kind == exp.Kind &&
                   EqualityComparer<Exp>.Default.Equals(Operand0, exp.Operand0) &&
                   EqualityComparer<Exp>.Default.Equals(Operand1, exp.Operand1);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Operand0, Operand1);
        }

        public static bool operator ==(BinaryOpExp? left, BinaryOpExp? right)
        {
            return EqualityComparer<BinaryOpExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(BinaryOpExp? left, BinaryOpExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is UnaryOpExp exp &&
                   Kind == exp.Kind &&
                   EqualityComparer<Exp>.Default.Equals(Operand, exp.Operand);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Operand);
        }

        public static bool operator ==(UnaryOpExp? left, UnaryOpExp? right)
        {
            return EqualityComparer<UnaryOpExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(UnaryOpExp? left, UnaryOpExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is CallExp exp &&
                   EqualityComparer<Exp>.Default.Equals(Callable, exp.Callable) &&
                   Enumerable.SequenceEqual(TypeArgs, exp.TypeArgs) &&
                   Enumerable.SequenceEqual(Args, exp.Args);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Callable, Args);
        }

        public static bool operator ==(CallExp? left, CallExp? right)
        {
            return EqualityComparer<CallExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(CallExp? left, CallExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is LambdaExpParam param &&
                   EqualityComparer<TypeExp?>.Default.Equals(Type, param.Type) &&
                   Name == param.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        public static bool operator ==(LambdaExpParam left, LambdaExpParam right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LambdaExpParam left, LambdaExpParam right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is LambdaExp exp &&
                   Enumerable.SequenceEqual(Params, exp.Params) &&
                   EqualityComparer<Stmt>.Default.Equals(Body, exp.Body);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Params, Body);
        }

        public static bool operator ==(LambdaExp? left, LambdaExp? right)
        {
            return EqualityComparer<LambdaExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(LambdaExp? left, LambdaExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is IndexerExp exp &&
                   EqualityComparer<Exp>.Default.Equals(Object, exp.Object) &&
                   EqualityComparer<Exp>.Default.Equals(Index, exp.Index);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object, Index);
        }

        public static bool operator ==(IndexerExp? left, IndexerExp? right)
        {
            return EqualityComparer<IndexerExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(IndexerExp? left, IndexerExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is MemberCallExp exp &&
                   EqualityComparer<Exp>.Default.Equals(Object, exp.Object) &&
                   MemberName == exp.MemberName &&
                   Enumerable.SequenceEqual(MemberTypeArgs, exp.MemberTypeArgs) &&
                   Enumerable.SequenceEqual(Args, exp.Args);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object, MemberName, Args);
        }

        public static bool operator ==(MemberCallExp? left, MemberCallExp? right)
        {
            return EqualityComparer<MemberCallExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(MemberCallExp? left, MemberCallExp? right)
        {
            return !(left == right);
        }
    }

    public class MemberExp : Exp
    {
        public Exp Object { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> MemberTypeArgs { get; }

        public MemberExp(Exp obj, string memberName, IEnumerable<TypeExp> memberTypeArgs)
        {
            Object = obj;
            MemberName = memberName;
            MemberTypeArgs = memberTypeArgs.ToImmutableArray();
        }

        public override bool Equals(object? obj)
        {
            return obj is MemberExp exp &&
                   EqualityComparer<Exp>.Default.Equals(Object, exp.Object) &&
                   MemberName == exp.MemberName &&
                   Enumerable.SequenceEqual(MemberTypeArgs, exp.MemberTypeArgs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object, MemberName);
        }

        public static bool operator ==(MemberExp? left, MemberExp? right)
        {
            return EqualityComparer<MemberExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(MemberExp? left, MemberExp? right)
        {
            return !(left == right);
        }
    }

    public class ListExp : Exp
    {
        TypeExp? ElemType { get; }
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

        public override bool Equals(object? obj)
        {
            return obj is ListExp exp &&
                   EqualityComparer<TypeExp?>.Default.Equals(ElemType, exp.ElemType) &&
                   Enumerable.SequenceEqual(Elems, exp.Elems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Elems);
        }

        public static bool operator ==(ListExp? left, ListExp? right)
        {
            return EqualityComparer<ListExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(ListExp? left, ListExp? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is NewExp exp &&
                   EqualityComparer<TypeExp>.Default.Equals(Type, exp.Type) &&
                   SeqEqComparer.Equals(Args, exp.Args);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Type);
            SeqEqComparer.AddHash(ref hashCode, Args);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(NewExp? left, NewExp? right)
        {
            return EqualityComparer<NewExp?>.Default.Equals(left, right);
        }

        public static bool operator !=(NewExp? left, NewExp? right)
        {
            return !(left == right);
        }
    }
}
