#pragma once

#include "RExp.h"
#include "RLocs.h"
#include "Symbol/MTypes.h"

namespace Citron
{
#pragma region Storage

// Location의 Value를 resultValue에 복사한다
class RLoadExp
{
    RLoc loc;
    MType type;
};

// a = b
class RAssignExp
{
    RLoc dest;
    RExp src;
};

// box 3
class RBoxExp
{
    RExp innerExp;
    MType innerType;
};

class RStaticBoxRefExp
{
    RLoc loc;
    MType locType;
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
class RClassMemberBoxRefExp
{
    RLoc holder;
    MClassMemberVar memberVar;
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
class RStructIndirectMemberBoxRefExp
{
    RLoc holder;
    MStructMemberVar memberVar;
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
class RStructMemberBoxRefExp
{
    RLoc parent;
    MStructMemberVar memberVar;
};

// &i
class RLocalRefExp
{
    RLoc innerLoc;
};

#pragma endregion Storage

#pragma region Interface

class RCastBoxedLambdaToFuncExp
{
    RExp exp;
    MFuncType funcType;
};

#pragma endregion Interface

#pragma region Literal

// false
class RBoolLiteralExp
{
    bool value;
};

// 1
class RIntLiteralExp
{
    int value;
};

class TextStringExpElement
{
    std::string text;
};

class ExpStringExpElement
{
    RLoc loc;
};

using RStringExpElement = std::variant<TextStringExpElement, ExpStringExpElement>;

// "dskfjslkf $abc "
class RStringExp
{
    std::vector<RStringExpElement> elements;
};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
class RListExp
{
    std::vector<RExp> elems;
    MType itemType;
};

class RListIteratorExp
{
    RLoc listLoc;
    MType iteratorType;
};

#pragma endregion List

#pragma region Call Internal

enum class RInternalUnaryOperator
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    ToString_Bool_String,
    ToString_Int_String,
};

enum class InternalUnaryAssignOperator
{
    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,
};

enum class RInternalBinaryOperator
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
};

class RCallInternalUnaryOperatorExp
{
    RInternalUnaryOperator op;
    RExp operand;
};

class RCallInternalUnaryAssignOperatorExp
{
    RInternalUnaryAssignOperator op;
    RLoc operand;
};

class RCallInternalBinaryOperatorExp
{
    RInternalBinaryOperator op;
    RExp operand0;
    RExp operand1;
};

#pragma endregion Call Internal

#pragma region Global

// F();
class RCallGlobalFuncExp
{
    MGlobalFuncSymbol func;
    std::vector<RArgument> args;
};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
class RNewClassExp
{
    MClassConstructorSymbol constructor;
    std::vector<RArgument> args;
};

// c.F();
class RCallClassMemberFuncExp
{
    MClassMemberFuncSymbol classMemberFunc;
    std::optional<RLoc> instance;
    std::vector<RArgument> args;
};

// ClassStaticCast
class RCastClassExp
{
    RExp src;
    MClass _class; // MType이 아니라도 괜찮은가
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
class RNewStructExp
{
    MStructConstructor constructor;
    std::vector<RArgument> args;
};

// s.F();
class RCallStructMemberFuncExp
{
    MStructMemberFunc structMemberFunc;
    std::optional<RLoc> instance;
    std::vector<RArgument> args;
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
class RNewEnumElemExp
{
    MEnumElem enumElem;
    std::vector<Argument> args;
};

// 컨테이너를 enumElem -> enum으로
class RCastEnumElemToEnumExp
{
    RExp src;
    MEnum _enum;
};

#pragma endregion Enum

#pragma region Nullable

class RNullableNullLiteralExp
{
    MType innerType;
};

class RNewNullableExp
{
    RExp innerExp;
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
class RLambdaExp
{
    MLambda lambda;
    std::vector<RArgument> args;
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
class RCallLambdaExp
{
    MLambda lambda;
    RLoc callable;
    std::vector<RArgument> args;
};

#pragma endregion Lambda

#pragma region Inline

class RInlineBlockExp
{
    std::vector<Stmt> stmts;
    MType returnType;
};

#pragma endregion Inline

#pragma region TypeTest

class RClassIsClassExp
{
    RExp exp;
    MClassType classType;
};

class RClassAsClassExp
{
    RExp exp;
    MClassType classType;
};

class RClassIsInterfaceExp
{
    RExp exp;
    MInterfaceType interfaceType;
};

class RClassAsInterfaceExp
{
    RExp exp;
    MInterfaceType interfaceType;
};

class RInterfaceIsClassExp
{
    RExp exp;
    MClassType classType;
};

class RInterfaceAsClassExp
{
    RExp exp;
    MClassType classType;
};

class RInterfaceIsInterfaceExp
{
    RExp exp;
    MInterfaceType interfaceType;
};

class RInterfaceAsInterfaceExp
{
    RExp exp;
    MInterfaceType interfaceType;
};

class REnumIsEnumElemExp
{
    RExp exp;
    MEnumElemType enumElemType;
};

class REnumAsEnumElemExp
{
    RExp exp;
    MEnumElemType enumElemType;
};

#pragma endregion TypeTest

}