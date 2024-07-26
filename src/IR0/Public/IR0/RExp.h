#pragma once
#include <variant>
#include <memory>
#include <string>
#include <vector>

#include "RArgument.h"
#include "RType.h"
#include "RClassMemberVar.h"

namespace Citron {

using RStmtPtr = std::unique_ptr<class RStmt>;
using RLocPtr = std::unique_ptr<class RLoc>;

// Storage
class RLoadExp;
class RAssignExp;

// BoxRef
class RBoxExp;
class RStaticBoxRefExp;
class RClassMemberBoxRefExp;
class RStructIndirectMemberBoxRefExp;
class RStructMemberBoxRefExp;

// LocalRef
class RLocalRefExp;

// Literal
class RBoolLiteralExp;
class RIntLiteralExp;
class RStringExp;

// List
class RListExp; 
class RListIteratorExp;

// Call Internal
class RCallInternalUnaryOperatorExp;
class RCallInternalUnaryAssignOperatorExp;
class RCallInternalBinaryOperatorExp;

// Global
class RCallGlobalFuncExp;

// Class
class RNewClassExp;
class RCallClassMemberFuncExp;
class RCastClassExp;

// Struct
class RNewStructExp;
class RCallStructMemberFuncExp;

// Enum
class RNewEnumElemExp;
class RCastEnumElemToEnumExp;

// Nullable
class RNewNullableExp;
class RNullableNullLiteralExp;

// Lambda
class RLambdaExp;
class RCallLambdaExp;

// Func
class RCastBoxedLambdaToFuncExp;

// InlineBlock
class RInlineBlockExp;

// Test
class RClassIsClassExp;
class RClassAsClassExp;
class RClassIsInterfaceExp;
class RClassAsInterfaceExp;
class RInterfaceIsClassExp;
class RInterfaceAsClassExp;
class RInterfaceIsInterfaceExp;
class RInterfaceAsInterfaceExp;
class REnumIsEnumElemExp;
class REnumAsEnumElemExp;

class RExpVisitor
{
public:
    virtual void Visit(RLoadExp& exp) = 0;
    virtual void Visit(RAssignExp& exp) = 0;
    virtual void Visit(RBoxExp& exp) = 0;
    virtual void Visit(RStaticBoxRefExp& exp) = 0;
    virtual void Visit(RClassMemberBoxRefExp& exp) = 0;
    virtual void Visit(RStructIndirectMemberBoxRefExp& exp) = 0;
    virtual void Visit(RStructMemberBoxRefExp& exp) = 0;
    virtual void Visit(RLocalRefExp& exp) = 0;
    virtual void Visit(RBoolLiteralExp& exp) = 0;
    virtual void Visit(RIntLiteralExp& exp) = 0;
    virtual void Visit(RStringExp& exp) = 0;
    virtual void Visit(RListExp& exp) = 0;
    virtual void Visit(RListIteratorExp& exp) = 0;
    virtual void Visit(RCallInternalUnaryOperatorExp& exp) = 0;
    virtual void Visit(RCallInternalUnaryAssignOperatorExp& exp) = 0;
    virtual void Visit(RCallInternalBinaryOperatorExp& exp) = 0;
    virtual void Visit(RCallGlobalFuncExp& exp) = 0;
    virtual void Visit(RNewClassExp& exp) = 0;
    virtual void Visit(RCallClassMemberFuncExp& exp) = 0;
    virtual void Visit(RCastClassExp& exp) = 0;
    virtual void Visit(RNewStructExp& exp) = 0;
    virtual void Visit(RCallStructMemberFuncExp& exp) = 0;
    virtual void Visit(RNewEnumElemExp& exp) = 0;
    virtual void Visit(RCastEnumElemToEnumExp& exp) = 0;
    virtual void Visit(RNewNullableExp& exp) = 0;
    virtual void Visit(RNullableNullLiteralExp& exp) = 0;
    virtual void Visit(RLambdaExp& exp) = 0;
    virtual void Visit(RCallLambdaExp& exp) = 0;
    virtual void Visit(RCastBoxedLambdaToFuncExp& exp) = 0;
    virtual void Visit(RInlineBlockExp& exp) = 0;
    virtual void Visit(RClassIsClassExp& exp) = 0;
    virtual void Visit(RClassAsClassExp& exp) = 0;
    virtual void Visit(RClassIsInterfaceExp& exp) = 0;
    virtual void Visit(RClassAsInterfaceExp& exp) = 0;
    virtual void Visit(RInterfaceIsClassExp& exp) = 0;
    virtual void Visit(RInterfaceAsClassExp& exp) = 0;
    virtual void Visit(RInterfaceIsInterfaceExp& exp) = 0;
    virtual void Visit(RInterfaceAsInterfaceExp& exp) = 0;
    virtual void Visit(REnumIsEnumElemExp& exp) = 0;
    virtual void Visit(REnumAsEnumElemExp& exp) = 0;
};

class RExp
{
public:
    virtual void Accept(RExpVisitor& visitor) = 0;
};

using RExpPtr = std::unique_ptr<RExp>;

#pragma region Storage

// Location의 Value를 resultValue에 복사한다
class RLoadExp : public RExp
{
    RLocPtr loc;
    RTypePtr type;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// a = b
class RAssignExp : public RExp
{
    RLocPtr dest;
    RExpPtr src;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box 3
class RBoxExp : public RExp
{
    RExpPtr innerExp;
    RTypePtr innerType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RStaticBoxRefExp : public RExp
{
    RLocPtr loc;
    RTypePtr locType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
class RClassMemberBoxRefExp : public RExp
{
    RLocPtr holder;
    std::shared_ptr<RClassMemberVar> memberVar;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
class RStructIndirectMemberBoxRefExp : public RExp
{
    RLocPtr holder;
    std::shared_ptr<RStructMemberVar> memberVar;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
class RStructMemberBoxRefExp : public RExp
{
    RLocPtr parent;
    std::shared_ptr<RStructMemberVar> memberVar;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &i
class RLocalRefExp : public RExp
{
    RLocPtr innerLoc;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Storage

#pragma region Interface

class RCastBoxedLambdaToFuncExp : public RExp
{
    RExpPtr exp;
    std::shared_ptr<RFuncType> funcType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Interface

#pragma region Literal

// false
class RBoolLiteralExp : public RExp
{
    bool value;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 1
class RIntLiteralExp : public RExp
{
    int value;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class TextStringExpElement
{
    std::string text;
};

class ExpStringExpElement
{
    RLocPtr loc;
};

using RStringExpElement = std::variant<TextStringExpElement, ExpStringExpElement>;

// "dskfjslkf $abc "
class RStringExp : public RExp
{
    std::vector<RStringExpElement> elements;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
class RListExp : public RExp
{
    std::vector<RExpPtr> elems;
    RTypePtr itemType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RListIteratorExp : public RExp
{
    RLocPtr listLoc;
    RTypePtr iteratorType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
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

enum class RInternalUnaryAssignOperator
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

class RCallInternalUnaryOperatorExp : public RExp
{
    RInternalUnaryOperator op;
    RExpPtr operand;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RCallInternalUnaryAssignOperatorExp : public RExp
{
    RInternalUnaryAssignOperator op;
    RLocPtr operand;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RCallInternalBinaryOperatorExp : public RExp
{
    RInternalBinaryOperator op;
    RExpPtr operand0;
    RExpPtr operand1;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Call Internal

#pragma region Global

// F();
class RCallGlobalFuncExp : public RExp
{
    std::shared_ptr<RGlobalFunc> func;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
class RNewClassExp : public RExp
{
    std::shared_ptr<RClassConstructor> constructor;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// c.F();
class RCallClassMemberFuncExp : public RExp
{
    std::shared_ptr<RClassMemberFunc> classMemberFunc;
    RLocPtr instance;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// ClassStaticCast
class RCastClassExp : public RExp
{
    RExpPtr src;
    std::shared_ptr<RClass> _class; // RType이 아니라도 괜찮은가
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
class RNewStructExp : public RExp
{
    std::shared_ptr<RStructConstructor> constructor;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// s.F();
class RCallStructMemberFuncExp : public RExp
{
    std::shared_ptr<RStructMemberFunc> structMemberFunc;
    RLocPtr instance;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
class RNewEnumElemExp : public RExp
{
    std::shared_ptr<REnumElem> enumElem;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 컨테이너를 enumElem -> enum으로
class RCastEnumElemToEnumExp : public RExp
{
    RExpPtr src;
    std::shared_ptr<REnum> _enum;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Enum

#pragma region Nullable

class RNullableNullLiteralExp : public RExp
{
    RTypePtr innerType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RNewNullableExp : public RExp
{
    RExpPtr innerExp;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
class RLambdaExp : public RExp
{
    std::shared_ptr<RLambda> lambda;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
class RCallLambdaExp : public RExp
{
    std::shared_ptr<RLambda> lambda;
    RLocPtr callable;
    std::vector<RArgument> args;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Lambda

#pragma region Inline

class RInlineBlockExp : public RExp
{
    std::vector<RStmtPtr> stmts;
    RTypePtr returnType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Inline

#pragma region TypeTest

class RClassIsClassExp : public RExp
{
    RExpPtr exp;
    RTypePtr classType;

public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RClassAsClassExp : public RExp
{
    RExpPtr exp;
    RTypePtr classType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RClassIsInterfaceExp : public RExp
{
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RClassAsInterfaceExp : public RExp
{
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RInterfaceIsClassExp : public RExp
{
    RExpPtr exp;
    RTypePtr classType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RInterfaceAsClassExp : public RExp
{
    RExpPtr exp;
    RTypePtr classType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RInterfaceIsInterfaceExp : public RExp
{
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RInterfaceAsInterfaceExp : public RExp
{
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class REnumIsEnumElemExp : public RExp
{
    RExpPtr exp;
    RTypePtr enumElemType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class REnumAsEnumElemExp : public RExp
{
    RExpPtr exp;
    RTypePtr enumElemType;
public:
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion TypeTest

}