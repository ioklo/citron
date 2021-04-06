using Gum.CompileTime;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gum.IR0
{
    public abstract class Exp
    {
        internal Exp() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalVarExp : Exp
    {
        public string Name { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class LocalVarExp : Exp
    {
        public string Name { get; } // LocalVarId;
    }

    // "dskfjslkf $abc "
    [AutoConstructor, ImplementIEquatable]
    public partial class StringExp : Exp
    {
        public ImmutableArray<StringExpElement> Elements { get; }
    }

    // 1
    [AutoConstructor, ImplementIEquatable]
    public partial class IntLiteralExp : Exp
    {
        public int Value { get; }
    }

    // false
    [AutoConstructor, ImplementIEquatable]
    public partial class BoolLiteralExp : Exp
    {
        public bool Value { get; }
    }

    public enum InternalUnaryOperator
    {
        LogicalNot_Bool_Bool,
        UnaryMinus_Int_Int,

        ToString_Bool_String,
        ToString_Int_String,
    }

    public enum InternalUnaryAssignOperator
    {
        PrefixInc_Int_Int,
        PrefixDec_Int_Int,
        PostfixInc_Int_Int,
        PostfixDec_Int_Int,
    }

    public enum InternalBinaryOperator
    {
        Multiply_Int_Int_Int,
        Divide_Int_Int_Int,
        Modulo_Int_Int_Int,
        Add_Int_Int_Int,
        Add_String_String_String,
        Subtract_Int_Int_Int,
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

    [AutoConstructor, ImplementIEquatable]
    public partial class CallInternalUnaryOperatorExp : Exp
    {
        public InternalUnaryOperator Operator { get; }
        public ExpInfo Operand { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallInternalUnaryAssignOperator : Exp
    {
        public InternalUnaryAssignOperator Operator { get; }
        public Exp Operand { get; }        
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallInternalBinaryOperatorExp : Exp
    {
        public InternalBinaryOperator Operator { get; }
        public ExpInfo Operand0 { get; }
        public ExpInfo Operand1 { get; }
    }

    // a = b
    [AutoConstructor, ImplementIEquatable]
    public partial class AssignExp : Exp
    {
        public Exp Dest { get; }
        public Exp Src { get; }
    }

    // F(2, 3)
    [AutoConstructor, ImplementIEquatable]
    public partial class CallFuncExp : Exp
    {
        public Func Func { get; }
        public ExpInfo? Instance { get; }
        public ImmutableArray<ExpInfo> Args { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallSeqFuncExp : Exp
    {
        public Func Func { get; }
        public ExpInfo? Instance { get; }
        public ImmutableArray<ExpInfo> Args { get; }
    }

    // f(2, 3)
    [AutoConstructor, ImplementIEquatable]
    public partial class CallValueExp : Exp
    {
        public ExpInfo Callable { get; }        
        public ImmutableArray<ExpInfo> Args { get; }
    }

    // () => { return 1; }
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaExp : Exp
    {
        public CaptureInfo CaptureInfo { get; }
        public ImmutableArray<string> ParamNames { get; }
        public Stmt Body { get; }
    }

    // l[b], l is list
    [AutoConstructor, ImplementIEquatable]
    public partial class ListIndexerExp : Exp
    {
        public ExpInfo ListInfo { get; }
        public ExpInfo IndexInfo { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class StaticMemberExp : Exp
    {
        public Type Type { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class StructMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ClassMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }
    }

    // [1, 2, 3]
    [AutoConstructor, ImplementIEquatable]
    public partial class ListExp : Exp
    {
        public Type ElemType { get; }
        public ImmutableArray<Exp> Elems { get; }
    }

    // enum construction, E.First or E.Second(2, 3)
    [AutoConstructor, ImplementIEquatable]
    public partial class NewEnumExp : Exp
    {
        [AutoConstructor, ImplementIEquatable]
        public partial struct Elem
        {
            public string Name { get; }
            public ExpInfo ExpInfo { get; }
        }

        public string Name { get; }
        public ImmutableArray<Elem> Members { get; }
    }

    // new S(2, 3, 4);
    [AutoConstructor, ImplementIEquatable]
    public partial class NewStructExp : Exp
    {
        public Type Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<ExpInfo> Args { get; }
    }

    // new C(2, 3, 4);
    [AutoConstructor, ImplementIEquatable]
    public partial class NewClassExp : Exp
    {
        public Type Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<ExpInfo> Args { get; }
    }
}
