using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gum.IR0
{
    public abstract class Exp
    {
        internal Exp() { }
    }

    public class ExternalGlobalVarExp : Exp
    {
        public ModuleItemId VarId { get; } // ExternalGlobalVarId
        public ExternalGlobalVarExp(ModuleItemId varId) { VarId = varId; }
    }

    public class PrivateGlobalVarExp : Exp
    {
        public string Name { get; } // GlobalVarId;
        public PrivateGlobalVarExp(string name) { Name = name; }
    }

    public class LocalVarExp : Exp
    {
        public string Name { get; } // LocalVarId;
        public LocalVarExp(string name) { Name = name; }
    }    

    public class EnumExp : Exp
    {
        public struct Elem
        {
            public string Name { get; }
            public Exp Exp { get; }
            public TypeValue Type { get; }

            public Elem(string name, Exp exp, TypeValue type)
            {
                Name = name;
                Exp = exp;
                Type = type;
            }
        }

        public string Name { get; }

        public ImmutableArray<Elem> Members { get; }

        public EnumExp(string name, IEnumerable<Elem> members)
        {
            Name = name;
            Members = members.ToImmutableArray();
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

    //public class BinaryOpExp : Exp
    //{
    //    public BinaryOpKind Kind { get; }
    //    public Exp Operand0 { get; }
    //    public Exp Operand1 { get; }
        
    //    public BinaryOpExp(BinaryOpKind kind, Exp operand0, Exp operand1)
    //    {
    //        Kind = kind;
    //        Operand0 = operand0;
    //        Operand1 = operand1;
    //    }

    //    public override bool Equals(object? obj)
    //    {
    //        return obj is BinaryOpExp exp &&
    //               Kind == exp.Kind &&
    //               EqualityComparer<Exp>.Default.Equals(Operand0, exp.Operand0) &&
    //               EqualityComparer<Exp>.Default.Equals(Operand1, exp.Operand1);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Kind, Operand0, Operand1);
    //    }

    //    public static bool operator ==(BinaryOpExp? left, BinaryOpExp? right)
    //    {
    //        return EqualityComparer<BinaryOpExp?>.Default.Equals(left, right);
    //    }

    //    public static bool operator !=(BinaryOpExp? left, BinaryOpExp? right)
    //    {
    //        return !(left == right);
    //    }
    //}

    public enum InternalOperator
    {
        LogicalNot_Bool_Bool,
        PrefixInc_Int_Void,
        PrefixDec_Int_Void,
        UnaryMinus_Int_Int,
        Multiply_Int_Int_Int,
        Divide_Int_Int_Int,
        Modulo_Int_Int_Int,
        Add_Int_Int_Int,
        Add_String_String_String,
        Substract_Int_Int_Int,
        LessThan_Int_Int_Bool,
        LessThan_String_String_Bool,
        GreaterThan_Int_Int_Bool,
        GreaterThan_String_String_Bool,
        LessThanOrEqual_Int_Int_Bool,
        LessThanOrEqual_String_String_Bool,
        GreaterThanOrEqual_Int_Int_Bool,
        GreaterThanOrEqual_String_String_Bool,
        Equal_Int_Int_Bool,
        Equal_Bool_Bool_Bool,
        Equal_String_String_Bool
    }

    public class CallInternalOperatorExp : Exp
    {
        public InternalOperator Operator { get; }
        public ImmutableArray<ExpAndType> Operands { get; }

        public CallInternalOperatorExp(InternalOperator op, IEnumerable<ExpAndType> operands) 
        { 
            Operator = op;
            Operands = operands.ToImmutableArray();
        }
    }

    public class PrefixExp : Exp
    {
        public Exp Operand { get; }
        public PrefixExp(Exp operand) { Operand = operand; }
    }

    public class PostfixExp : Exp
    {
        public Exp Operand { get; }
        public PostfixExp(Exp operand) { Operand = operand; }
    }

    public class AssignExp : Exp
    {
        public Exp Dest { get; }
        public Exp Src { get; }
        public TypeValue SrcType { get; }

        public AssignExp(Exp dest, Exp src, TypeValue srcType)
        {
            Dest = dest;
            Src = src;
            SrcType = srcType;
        }

    }

    //public class UnaryOpExp : Exp
    //{
    //    public UnaryOpKind Kind { get; }
    //    public Exp Operand{ get; }
    //    public UnaryOpExp(UnaryOpKind kind, Exp operand)
    //    {
    //        Kind = kind;
    //        Operand = operand;
    //    }

    //    public override bool Equals(object? obj)
    //    {
    //        return obj is UnaryOpExp exp &&
    //               Kind == exp.Kind &&
    //               EqualityComparer<Exp>.Default.Equals(Operand, exp.Operand);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Kind, Operand);
    //    }

    //    public static bool operator ==(UnaryOpExp? left, UnaryOpExp? right)
    //    {
    //        return EqualityComparer<UnaryOpExp?>.Default.Equals(left, right);
    //    }

    //    public static bool operator !=(UnaryOpExp? left, UnaryOpExp? right)
    //    {
    //        return !(left == right);
    //    }
    //}

    public struct ExpAndType
    {
        public Exp Exp { get; }
        public TypeValue TypeValue { get; }

        public ExpAndType(Exp exp, TypeValue typeValue)
        {
            Exp = exp;
            TypeValue = typeValue;
        }
    }

    public class CallFuncExp : Exp
    {
        public FuncValue FuncValue { get; }
        public (Exp Exp, TypeValue Type)? Instance { get; }
        public ImmutableArray<ExpAndType> Args { get; }

        public CallFuncExp(FuncValue funcValue, (Exp Exp, TypeValue Type)? instance, IEnumerable<ExpAndType> args)
        {
            FuncValue = funcValue;
            Instance = instance;
            Args = args.ToImmutableArray();
        }
    }

    public class CallValueExp : Exp
    {
        public Exp CallableExp { get; }
        public TypeValue CallableType { get; }
        public ImmutableArray<ExpAndType> Args { get; }

        public CallValueExp(Exp callableExp, TypeValue callableType, IEnumerable<ExpAndType> args)
        {
            CallableExp = callableExp;
            CallableType = callableType;
            Args = args.ToImmutableArray();
        }




    }


    //public class CallExp : Exp
    //{
    //    public Exp Callable { get; }

    //    public ImmutableArray<TypeValue> TypeArgs { get; }

    //    // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
    //    public ImmutableArray<Exp> Args { get; }

    //    public CallExp(Exp callable, IEnumerable<TypeValue> typeArgs, IEnumerable<Exp> args)
    //    {
    //        Callable = callable;
    //        TypeArgs = typeArgs.ToImmutableArray();
    //        Args = args.ToImmutableArray();
    //    }

    //    public CallExp(Exp callable, IEnumerable<TypeValue> typeArgs, params Exp[] args)
    //    {
    //        Callable = callable;
    //        TypeArgs = typeArgs.ToImmutableArray();
    //        Args = ImmutableArray.Create(args);
    //    }

    //    public override bool Equals(object? obj)
    //    {
    //        return obj is CallExp exp &&
    //               EqualityComparer<Exp>.Default.Equals(Callable, exp.Callable) &&
    //               Enumerable.SequenceEqual(TypeArgs, exp.TypeArgs) &&
    //               Enumerable.SequenceEqual(Args, exp.Args);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Callable, Args);
    //    }

    //    public static bool operator ==(CallExp? left, CallExp? right)
    //    {
    //        return EqualityComparer<CallExp?>.Default.Equals(left, right);
    //    }

    //    public static bool operator !=(CallExp? left, CallExp? right)
    //    {
    //        return !(left == right);
    //    }
    //}
        
    public class LambdaExp : Exp
    {
        public CaptureInfo CaptureInfo { get; }
        public Stmt Body { get; }

        public LambdaExp(CaptureInfo captureInfo, Stmt body)
        {
            CaptureInfo = captureInfo;
            Body = body;
        }
    }
    
    // a[b]
    public class IndexerExp : Exp
    {
        public FuncValue FuncValue { get; }

        public Exp Object { get; }
        public TypeValue ObjectType { get; }
        public Exp Index { get; }
        public TypeValue IndexType { get; }


        public IndexerExp(FuncValue funcValue, Exp obj, TypeValue objType, Exp index, TypeValue indexType)
        {
            FuncValue = funcValue;
            Object = obj;
            ObjectType = objType;
            Index = index;
            IndexType = indexType;
        }
    }

    public class MemberCallExp : Exp
    {
        public Exp Object { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeValue> MemberTypeArgs { get; }
        public ImmutableArray<Exp> Args { get; }

        public MemberCallExp(Exp obj, string memberName, IEnumerable<TypeValue> typeArgs, IEnumerable<Exp> args)
        {
            Object = obj;
            MemberName = memberName;
            MemberTypeArgs = typeArgs.ToImmutableArray();
            Args = args.ToImmutableArray();
        }

        public MemberCallExp(Exp obj, string memberName, IEnumerable<TypeValue> typeArgs, params Exp[] args)
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
        public ImmutableArray<TypeValue> MemberTypeArgs { get; }

        public MemberExp(Exp obj, string memberName, IEnumerable<TypeValue> memberTypeArgs)
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
        public TypeValue ElemType { get; }
        public ImmutableArray<Exp> Elems { get; }

        public ListExp(TypeValue elemType, IEnumerable<Exp> elems)
        {
            ElemType = elemType;
            Elems = elems.ToImmutableArray();
        }
    }

    // new Type(2, 3, 4);
    public class NewExp : Exp
    {
        public TypeValue Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }
        
        public NewExp(TypeValue type, IEnumerable<Exp> args)
        {
            Type = type;
            Args = args.ToImmutableArray();
        }

        public override bool Equals(object? obj)
        {
            return obj is NewExp exp &&
                   EqualityComparer<TypeValue>.Default.Equals(Type, exp.Type) &&
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
