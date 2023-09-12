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
    public interface IBuiltInTypeProvider
    {
        IType GetBoolType();
        IType GetIntType();
        IType GetStringType();
    }

    public abstract record class Exp : INode
    {
        internal Exp() { }
        public abstract IType GetExpType(IBuiltInTypeProvider provider);
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, IIR0ExpVisitor<TResult>;
    }

    #region Storage

    // Location의 Value를 resultValue에 복사한다
    public record class LoadExp(Loc Loc, IType Type) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLoad(this);        
    }

    // a = b
    public record class AssignExp(Loc Dest, Exp Src) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return Src.GetExpType(provider);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitAssign(this);
    }

    // box 3
    public record class BoxExp(Exp InnerExp) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new BoxPtrType(InnerExp.GetExpType(provider));
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxExp(this);
    }

    public record class StaticBoxRefExp(Loc Loc, IType LocType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new BoxPtrType(LocType);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStaticBoxRef(this);
    }

    public record class ClassMemberBoxRefExp(Loc holderLoc, ClassMemberVarSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new BoxPtrType(Symbol.GetDeclType());
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberBoxRef(this);
    }

    public record class StructIndirectMemberBoxRefExp(Exp holderExp, StructMemberVarSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new BoxPtrType(Symbol.GetDeclType());
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructIndirectMemberBoxRef(this);
    }

    public record class StructMemberBoxRefExp(Exp Parent, StructMemberVarSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new BoxPtrType(Symbol.GetDeclType());
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberBoxRef(this);
    }

    // &i
    public record class LocalRefExp(Loc InnerLoc, IType InnerLocType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new LocalPtrType(InnerLocType);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalRef(this);
    }

    #endregion

    #region Interface

    // 이거 BoxedStructAsInterface
    public record class CastBoxedLambdaToFuncExp(Exp Exp, FuncType FuncType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return FuncType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.CastBoxedLambdaToFunc(this);
    }

    #endregion Interface

    #region Literal

    // false
    public record class BoolLiteralExp(bool Value) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoolLiteral(this);
    }

    // 1    
    public record class IntLiteralExp(int Value) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetIntType();
        }
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIntLiteral(this);
    }

    // "dskfjslkf $abc "
    public record class StringExp(ImmutableArray<StringExpElement> Elements) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetStringType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitString(this);
    }
    #endregion Literal

    #region List

    // [1, 2, 3]    
    public record class ListExp(ImmutableArray<Exp> Elems, IType ListType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ListType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitList(this);
    }

    public record class ListIteratorExp(Loc ListLoc, IType IteratorType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallInternalUnaryOperator(this);
    }

    public record CallInternalUnaryAssignOperatorExp(InternalUnaryAssignOperator Operator, Loc Operand, IType Type) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return Type;        
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallInternalUnaryAssignOperator(this);
    }

    public record CallInternalBinaryOperatorExp(InternalBinaryOperator Operator, Exp Operand0, Exp Operand1, IType Type) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ((ITypeSymbol)Constructor.GetOuter()).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewClass(this);
    }

    // c.F();
    public record CallClassMemberFuncExp(ClassMemberFuncSymbol ClassMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ClassMemberFunc.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallClassMemberFunc(this);
    }

    // ClassStaticCast    
    public record CastClassExp(Exp Src, ClassSymbol Class) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ((ITypeSymbol)Constructor.GetOuter()).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewStruct(this);
    }

    // s.F();
    public record CallStructMemberFuncExp(StructMemberFuncSymbol StructMemberFunc, Loc? Instance, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ((ITypeSymbol)EnumElem).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNewEnumElem(this);
    }

    // 컨테이너를 enumElem -> enum으로
    public record CastEnumElemToEnumExp(Exp Src, EnumSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new EnumType(Symbol);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCastEnumElemToEnum(this);
    }
    #endregion Enum

    #region Nullable
    public record class NullableNullLiteralExp(IType innerType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new NullableType(innerType);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNullableNullLiteral(this);
    }

    public record class NewNullableExp(Exp InnerExp) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new NullableType(InnerExp.GetExpType(provider));
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
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ((ITypeSymbol)Lambda).MakeType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambda(this);
    }

    // f(2, 3)    
    // Callable은 (() => {}) ()때문에 Loc이어야 한다
    public record class CallLambdaExp(LambdaSymbol Lambda, Loc Callable, ImmutableArray<Argument> Args) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return Lambda.GetReturn().Type;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCallValue(this);
    }

    #endregion

    #region Inline

    public record class InlineBlockExp(ImmutableArray<Stmt> Stmts, IType ReturnType) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return ReturnType;
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitInlineBlock(this);
    }

    #endregion Inline

    #region TypeTest
    public record class ClassIsClassExp(Exp Exp, ClassSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassIsClassExp(this);
    }

    public record class ClassAsClassExp(Exp Exp, ClassSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new ClassType(Symbol);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassAsClassExp(this);
    }

    public record class ClassIsInterfaceExp(Exp Exp, InterfaceSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassIsInterfaceExp(this);
    }

    public record class ClassAsInterfaceExp(Exp Exp, InterfaceSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new InterfaceType(Symbol);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassAsInterfaceExp(this);
    }

    public record class InterfaceIsClassExp(Exp Exp, ClassSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitInterfaceIsClassExp(this);
    }

    public record class InterfaceAsClassExp(Exp Exp, ClassSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new ClassType(Symbol);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitInterfaceAsClassExp(this);
    }

    public record class InterfaceIsInterfaceExp(Exp Exp, InterfaceSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitInterfaceIsInterfaceExp(this);
    }

    public record class InterfaceAsInterfaceExp(Exp Exp, InterfaceSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new InterfaceType(Symbol);
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitInterfaceAsInterfaceExp(this);
    }

    public record class EnumIsEnumElemExp(Exp Exp, EnumElemSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return provider.GetBoolType();
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumIsEnumElemExp(this);
    }

    public record class EnumAsEnumElemExp(Exp Exp, EnumElemSymbol Symbol) : Exp
    {
        public override IType GetExpType(IBuiltInTypeProvider provider)
        {
            return new NullableType(new EnumElemType(Symbol));
        }

        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumAsEnumElemExp(this);
    }

    #endregion TypeTest
}
