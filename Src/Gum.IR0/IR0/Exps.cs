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
    public abstract record Exp : INode
    {
        internal Exp() { }
    }

    // Location의 Value를 resultValue에 복사한다
    public record LoadExp(Loc Loc) : Exp;
    
    // "dskfjslkf $abc "
    public record StringExp(ImmutableArray<StringExpElement> Elements) : Exp;

    // 1    
    public record IntLiteralExp(int Value) : Exp;

    // false
    public record BoolLiteralExp(bool Value) : Exp;
    
    // 한개로 합쳐서 만듭니다
    // TODO: 'New' 단어는 힙할당에만 쓰도록 하자
    public record NewNullableExp(Path InnerType, Exp? ValueExp) : Exp;
    
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

    public record CallInternalUnaryOperatorExp(InternalUnaryOperator Operator, Exp Operand) : Exp;
    public record CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator Operator, Loc Operand) : Exp;
    public record CallInternalBinaryOperatorExp(InternalBinaryOperator Operator, Exp Operand0, Exp Operand1) : Exp;
    
    // a = b
    public record AssignExp(Loc Dest, Exp Src) : Exp;

    // F();
    public record CallGlobalFuncExp(Path.Nested Func, ImmutableArray<Argument> Args) : Exp;

    // c.F();
    public record CallClassMemberFuncExp(Path.Nested ClassMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp;

    // s.F();
    public record CallStructMemberFuncExp(Path.Nested StructMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp;
    
    //public record CallSeqFuncExp(Path.Nested SeqFunc , Loc? Instance, ImmutableArray<Argument> Args): Exp
    //{   
    //    // public bool NeedHeapAlloc { get; } Heap으로 할당시킬지 여부
    //}

    // f(2, 3)    
    // Callable은 (() => {}) ()때문에 Loc이어야 한다
    public record CallValueExp(Path.Nested Lambda, Loc Callable, ImmutableArray<Argument> Args) : Exp;

    // () => { return 1; }
    // 문법에서 LambdaExp는 지역적으로 보이지만, 생성된 람다는 프로그램 실행시 전역적으로 호출될 수 있기 때문에
    // LamdaExp안에 캡쳐할 정보와 Body 등을 바로 넣지 않고 Path로 접근한다    
    public record LambdaExp(Path.Nested Lambda) : Exp;
    
    // [1, 2, 3]    
    public record ListExp(Path ElemType, ImmutableArray<Exp> Elems) : Exp;    
    public record ListIteratorExp(Loc ListLoc) : Exp;

    // enum construction, E.First or E.Second(2, 3)    
    public record NewEnumElemExp(Path.Nested Elem, ImmutableArray<Argument> Args) : Exp;

    // new S(2, 3, 4);
    public record NewStructExp(Path.Nested Constructor, ImmutableArray<Argument> Args) : Exp;

    // new C(2, 3, 4);    
    public record NewClassExp(Path.Nested Class, ParamHash ConstructorParamHash, ImmutableArray<Argument> Args) : Exp;

    // 컨테이너를 enumElem -> enum으로
    public record CastEnumElemToEnumExp(Exp Src, Path.Nested EnumElem) : Exp;
    
    // ClassStaticCast    
    public record CastClassExp(Exp Src, Path Class) : Exp;
}
