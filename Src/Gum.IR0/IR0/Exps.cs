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

    // Location의 Value를 resultValue에 복사한다
    [AutoConstructor, ImplementIEquatable] 
    public partial class LoadExp : Exp
    {
        public Loc Loc { get; }
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
        public Exp Operand { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallInternalUnaryAssignOperator : Exp
    {
        public InternalUnaryAssignOperator Operator { get; }
        public Loc Operand { get; }        
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallInternalBinaryOperatorExp : Exp
    {
        public InternalBinaryOperator Operator { get; }
        public Exp Operand0 { get; }
        public Exp Operand1 { get; }
    }

    // a = b
    [AutoConstructor, ImplementIEquatable]
    public partial class AssignExp : Exp
    {
        public Loc Dest { get; }
        public Exp Src { get; }
    }

    // F(2, 3)
    [AutoConstructor, ImplementIEquatable]
    public partial class CallFuncExp : Exp
    {
        public Func Func { get; }
        public Loc? Instance { get; }
        public ImmutableArray<Exp> Args { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class CallSeqFuncExp : Exp
    {
        public DeclId DeclId { get; }
        public ImmutableArray<Type> TypeArgs { get; }
        public Loc? Instance { get; }
        public ImmutableArray<Exp> Args { get; }
        // public bool NeedHeapAlloc { get; } Heap으로 할당시킬지 여부
    }

    // f(2, 3)
    [AutoConstructor, ImplementIEquatable]
    public partial class CallValueExp : Exp
    {
        public Loc Callable { get; } // (() => {}) ()때문에 Loc이어야 한다
        public ImmutableArray<Exp> Args { get; }
    }

    // () => { return 1; }
    // 문법에서 LambdaExp는 지역적으로 보이지만, 생성된 람다는 프로그램 실행시 전역적으로 호출될 수 있기 때문에
    // LamdaExp안에 캡쳐할 정보와 Body 등을 바로 넣지 않고 LambdaDecl에 따로 보관한다
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaExp : Exp
    {
        public DeclId LambdaDeclId { get; }
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
            public Exp Exp { get; }
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
        public ImmutableArray<Exp> Args { get; }
    }

    // new C(2, 3, 4);
    [AutoConstructor, ImplementIEquatable]
    public partial class NewClassExp : Exp
    {
        public Type Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<Exp> Args { get; }
    }
}
