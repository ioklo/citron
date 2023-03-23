using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Citron.Symbol;

namespace Citron.IR0
{
    public abstract record class Exp : INode
    {
        internal Exp() { }
        public abstract IType GetExpType();
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, IIR0ExpVisitor<TResult>;
    }

    #region Storage

    // Location의 Value를 resultValue에 복사한다
    public record class LoadExp(Loc Loc, IType Type) : Exp
    {
        public override IType GetExpType()
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLoad(this);        
    }

    // a = b
    public record class AssignExp(Loc Dest, Exp Src) : Exp
    {
        public override IType GetExpType()
        {
            return Src.GetExpType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitAssign(this);
    }

    #endregion

    #region Literal

    // false
    public record class BoolLiteralExp(bool Value, IType BoolType) : Exp
    {
        public override IType GetExpType()
        {
            return BoolType;
        }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoolLiteral(this);
    }

    // 1    
    public record class IntLiteralExp(int Value, IType IntType) : Exp
    {
        public override IType GetExpType()
        {
            return IntType;
        }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIntLiteral(this);
    }

    // "dskfjslkf $abc "
    public record class StringExp(ImmutableArray<StringExpElement> Elements, IType StringType) : Exp
    {
        public override IType GetExpType()
        {
            return StringType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitString(this);
    }
    #endregion Literal

    #region List

    // [1, 2, 3]    
    public record class ListExp(ImmutableArray<Exp> Elems, IType ListType) : Exp
    {
        public override IType GetExpType()
        {
            return ListType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitList(this);
    }

    public record class ListIteratorExp(Loc ListLoc, IType IteratorType) : Exp
    {
        public override IType GetExpType()
        {
            return IteratorType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIterator(this);
    }

    #endregion List

    #region Call Internal

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

    public record CallInternalUnaryOperatorExp(InternalUnaryOperator Operator, Exp Operand, IType Type) : Exp
    {
        public override IType GetExpType()
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallInternalUnaryOperator(this);
    }

    public record CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator Operator, Loc Operand, IType Type) : Exp
    {
        public override IType GetExpType()
        {
            return Type;        
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallInternalUnaryAssignOperator(this);
    }

    public record CallInternalBinaryOperatorExp(InternalBinaryOperator Operator, Exp Operand0, Exp Operand1, IType Type) : Exp
    {
        public override IType GetExpType()
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallInternalBinaryOperator(this);
    }

    #endregion Call Internal

    #region Global
    // F();
    public record CallGlobalFuncExp(GlobalFuncSymbol Func, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return Func.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallGlobalFunc(this);
    }
    #endregion Global

    #region Class
    // new C(2, 3, 4);    
    public record class NewClassExp(ClassConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)Constructor.GetOuter()).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewClass(this);
    }

    // c.F();
    public record CallClassMemberFuncExp(ClassMemberFuncSymbol ClassMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return ClassMemberFunc.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallClassMemberFunc(this);
    }

    // ClassStaticCast    
    public record CastClassExp(Exp Src, ClassSymbol Class) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)Class).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCastClass(this);
    }

    #endregion Class

    #region Struct
    // new S(2, 3, 4);
    public record class NewStructExp(StructConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)Constructor.GetOuter()).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewStruct(this);
    }

    // s.F();
    public record CallStructMemberFuncExp(StructMemberFuncSymbol StructMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return StructMemberFunc.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallStructMemberFunc(this);
    }
    #endregion Struct

    #region Enum
    // enum construction, E.First or E.Second(2, 3)    
    public record class NewEnumElemExp(EnumElemSymbol EnumElem, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)EnumElem).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewEnumElem(this);
    }

    // 컨테이너를 enumElem -> enum으로
    public record CastEnumElemToEnumExp(Exp Src, EnumElemSymbol EnumElem) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)EnumElem).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCastEnumElemToEnum(this);
    }
    #endregion Enum

    #region Nullable

    // TODO: 'New' 단어는 힙할당에만 쓰도록 하자
    public record class NewNullableExp(Exp? ValueExp, IType Type) : Exp
    {
        public override IType GetExpType()
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewNullable(this);
    }

    #endregion Nullable

    #region Lambda

    // int x = 1;
    // var l = () => { return x; }; // lambda type
    // 
    // Lambda(lambda_type_0, x); // with captured variable
    public record class LambdaExp(LambdaSymbol Lambda, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return ((ITypeSymbol)Lambda).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambda(this);
    }

    // f(2, 3)    
    // Callable은 (() => {}) ()때문에 Loc이어야 한다
    public record class CallValueExp(LambdaSymbol Lambda, Loc Callable, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType()
        {
            return Lambda.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallValue(this);
    }

    // 이름은 'Func'인데 func<>를 뜻한다
    public record class CallFuncRefExp(Loc Callable, ImmutableArray<Argument> Args, FuncType FuncType) : Exp
    {
        public override IType GetExpType()
        {
            return FuncType.GetReturnType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallFuncRef(this);
    }
    
    // boxed lambda를 func<>(box)로 변환한다
    public record class CastBoxLambdaToFuncRefExp(Exp BoxLambdaSrc, FuncType FuncType) : Exp
    {
        public override IType GetExpType()
        {
            return FuncType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.CastBoxLambdaToFuncRef(this);
    }
    #endregion

    #region Ref

    // value로부터 box ref 생성
    // box 3
    public record class BoxExp(Exp Exp) : Exp
    {
        public override IType GetExpType()
        {
            return new BoxRefType(Exp.GetExpType());
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBox(this);
    }
    
    // &i, 대입하려는 대상에 따라서 Box인지 Ref인지 결정한다
    // int& = i; // localRef(local(i))
    public record class LocalRefExp(Loc InnerLoc, IType InnerLocType) : Exp
    {
        public override IType GetExpType()
        {
            return new LocalRefType(InnerLocType);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalRef(this);
    }

    // box int& = &i;
    public record class BoxRefExp(Loc Loc, IType LocType) : Exp
    {
        public override IType GetExpType()
        {
            return new BoxRefType(LocType);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxRef(this);
    }

    #endregion

    //public record CallSeqFuncExp(Path.Nested SeqFunc , Loc? Instance, ImmutableArray<Argument> Args): Exp
    //{   
    //    // public bool NeedHeapAlloc { get; } Heap으로 할당시킬지 여부
    //}
}
