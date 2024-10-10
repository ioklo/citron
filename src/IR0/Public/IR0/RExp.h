#pragma once
#include "IR0Config.h"

#include <variant>
#include <memory>
#include <string>
#include <vector>

#include "RArgument.h"
#include "RType.h"

namespace Citron {

using RStmtPtr = std::shared_ptr<class RStmt>;
using RLocPtr = std::shared_ptr<class RLoc>;

// Storage
class RExp_Load;
class RExp_Assign;

// BoxRef
class RExp_Box;
class RExp_StaticBoxRef;
class RExp_ClassMemberBoxRef;
class RExp_StructIndirectMemberBoxRef;
class RExp_StructMemberBoxRef;

// LocalRef
class RExp_LocalRef;

// Literal
class RExp_BoolLiteral;
class RExp_IntLiteral;
class RExp_String;

// List
class RExp_List; 
class RExp_ListIterator;

// Call Internal
class RExp_CallInternalUnaryOperator;
class RExp_CallInternalUnaryAssignOperator;
class RExp_CallInternalBinaryOperator;

// Global
class RExp_CallGlobalFunc;

// Class
class RExp_NewClass;
class RExp_CallClassMemberFunc;
class RExp_CastClass;

// Struct
class RExp_NewStruct;
class RExp_CallStructMemberFunc;

// Enum
class RExp_NewEnumElem;
class RExp_CastEnumElemToEnum;

// Nullable
class RExp_NewNullable;
class RExp_NullableValueNullLiteral;
class RExp_NullableRefNullLiteral;

// Lambda
class RExp_Lambda;
class RExp_CallLambda;

// Func
class RExp_CastBoxedLambdaToFunc;

// InlineBlock
class RExp_InlineBlock;

// Test
class RExp_ClassIsClass;
class RExp_ClassAsClass;
class RExp_ClassIsInterface;
class RExp_ClassAsInterface;
class RExp_InterfaceIsClass;
class RExp_InterfaceAsClass;
class RExp_InterfaceIsInterface;
class RExp_InterfaceAsInterface;
class RExp_EnumIsEnumElem;
class RExp_EnumAsEnumElem;

class RClassMemberVarDecl;
class RStructMemberVarDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassMemberFuncDecl;
class RClassConstructorDecl;
class RStructConstructorDecl;
class RStructMemberFuncDecl;
class RLambdaDecl;
class REnumDecl;
class REnumElemDecl;

class RExpVisitor
{
public:
    virtual void Visit(RExp_Load& exp) = 0;
    virtual void Visit(RExp_Assign& exp) = 0;
    virtual void Visit(RExp_Box& exp) = 0;
    virtual void Visit(RExp_StaticBoxRef& exp) = 0;
    virtual void Visit(RExp_ClassMemberBoxRef& exp) = 0;
    virtual void Visit(RExp_StructIndirectMemberBoxRef& exp) = 0;
    virtual void Visit(RExp_StructMemberBoxRef& exp) = 0;
    virtual void Visit(RExp_LocalRef& exp) = 0;
    virtual void Visit(RExp_BoolLiteral& exp) = 0;
    virtual void Visit(RExp_IntLiteral& exp) = 0;
    virtual void Visit(RExp_String& exp) = 0;
    virtual void Visit(RExp_List& exp) = 0;
    virtual void Visit(RExp_ListIterator& exp) = 0;
    virtual void Visit(RExp_CallInternalUnaryOperator& exp) = 0;
    virtual void Visit(RExp_CallInternalUnaryAssignOperator& exp) = 0;
    virtual void Visit(RExp_CallInternalBinaryOperator& exp) = 0;
    virtual void Visit(RExp_CallGlobalFunc& exp) = 0;
    virtual void Visit(RExp_NewClass& exp) = 0;
    virtual void Visit(RExp_CallClassMemberFunc& exp) = 0;
    virtual void Visit(RExp_CastClass& exp) = 0;
    virtual void Visit(RExp_NewStruct& exp) = 0;
    virtual void Visit(RExp_CallStructMemberFunc& exp) = 0;
    virtual void Visit(RExp_NewEnumElem& exp) = 0;
    virtual void Visit(RExp_CastEnumElemToEnum& exp) = 0;
    virtual void Visit(RExp_NewNullable& exp) = 0;

    virtual void Visit(RExp_NullableValueNullLiteral& exp) = 0;
    virtual void Visit(RExp_NullableRefNullLiteral& exp) = 0;

    virtual void Visit(RExp_Lambda& exp) = 0;
    virtual void Visit(RExp_CallLambda& exp) = 0;
    virtual void Visit(RExp_CastBoxedLambdaToFunc& exp) = 0;
    virtual void Visit(RExp_InlineBlock& exp) = 0;
    virtual void Visit(RExp_ClassIsClass& exp) = 0;
    virtual void Visit(RExp_ClassAsClass& exp) = 0;
    virtual void Visit(RExp_ClassIsInterface& exp) = 0;
    virtual void Visit(RExp_ClassAsInterface& exp) = 0;
    virtual void Visit(RExp_InterfaceIsClass& exp) = 0;
    virtual void Visit(RExp_InterfaceAsClass& exp) = 0;
    virtual void Visit(RExp_InterfaceIsInterface& exp) = 0;
    virtual void Visit(RExp_InterfaceAsInterface& exp) = 0;
    virtual void Visit(RExp_EnumIsEnumElem& exp) = 0;
    virtual void Visit(RExp_EnumAsEnumElem& exp) = 0;
};

class RExp
{
public:
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
    virtual void Accept(RExpVisitor& visitor) = 0;
};

using RExpPtr = std::shared_ptr<RExp>;

#pragma region Storage

// Location의 Value를 resultValue에 복사한다
class RExp_Load : public RExp
{
public:
    RLocPtr loc;
    
public:
    IR0_API RExp_Load(RLocPtr&& loc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// a = b
class RExp_Assign : public RExp
{
public:
    RLocPtr dest;
    RExpPtr src;
public:
    IR0_API RExp_Assign(RLocPtr&& dest, RExpPtr&& src);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box 3
class RExp_Box : public RExp
{
public:
    RExpPtr innerExp;    
public:
    IR0_API RExp_Box(RExpPtr&& innerExp);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &C.x
class RExp_StaticBoxRef : public RExp
{
public:
    RLocPtr loc;    
public:
    IR0_API RExp_StaticBoxRef(const RLocPtr& loc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
class RExp_ClassMemberBoxRef : public RExp
{
public:
    RLocPtr holder;
    std::shared_ptr<RClassMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RExp_ClassMemberBoxRef(const RLocPtr& holder, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
class RExp_StructIndirectMemberBoxRef : public RExp
{
public:
    RLocPtr holder;
    std::shared_ptr<RStructMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RExp_StructIndirectMemberBoxRef(const RLocPtr& holder, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
class RExp_StructMemberBoxRef : public RExp
{
public:
    RLocPtr parent;
    std::shared_ptr<RStructMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RExp_StructMemberBoxRef(const RLocPtr& parent, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &i
class RExp_LocalRef : public RExp
{
public:
    RLocPtr innerLoc;

public:
    IR0_API RExp_LocalRef(const RLocPtr& innerLoc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Storage

#pragma region Interface

// func<int, int> f = box (int x) => x + p;
class RExp_CastBoxedLambdaToFunc : public RExp
{
public:
    RExpPtr exp;
    std::shared_ptr<RType_Func> funcType;
public:
    IR0_API RExp_CastBoxedLambdaToFunc(const RExpPtr& exp, const std::shared_ptr<RType_Func>& funcType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Interface

#pragma region Literal

// false
class RExp_BoolLiteral : public RExp
{
public:
    bool value;
public:
    IR0_API RExp_BoolLiteral(bool value);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 1
class RExp_IntLiteral : public RExp
{
public:
    int value;
public:
    IR0_API RExp_IntLiteral(int value);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RTextStringExpElement
{
public:
    std::string text;

public:
    IR0_API RTextStringExpElement(const std::string& text);
};

class RLocStringExpElement
{
public:
    RLocPtr loc;

public:
    IR0_API RLocStringExpElement(RLocPtr&& loc);
};

using RStringExpElement = std::variant<RTextStringExpElement, RLocStringExpElement>;

// "dskfjslkf $abc "
class RExp_String : public RExp
{
public:
    std::vector<RStringExpElement> elements;

public:
    IR0_API RExp_String(std::vector<RStringExpElement>&& elements);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
    
};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
class RExp_List : public RExp
{
public:
    std::vector<RExpPtr> elems;
    RTypePtr itemType;
public:
    IR0_API RExp_List(std::vector<RExpPtr>&& elems, const RTypePtr& itemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_ListIterator : public RExp
{
public:
    RLocPtr listLoc;
    RTypePtr type;
public:
    IR0_API RExp_ListIterator(const RLocPtr& listLoc, const RTypePtr& type);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
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

class RExp_CallInternalUnaryOperator : public RExp
{
public:
    RInternalUnaryOperator op;
    RExpPtr operand;
public:
    IR0_API RExp_CallInternalUnaryOperator(RInternalUnaryOperator op, RExpPtr&& operand);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_CallInternalUnaryAssignOperator : public RExp
{
public:
    RInternalUnaryAssignOperator op;
    RLocPtr operand;
public:
    IR0_API RExp_CallInternalUnaryAssignOperator(RInternalUnaryAssignOperator op, RLocPtr&& operand);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_CallInternalBinaryOperator : public RExp
{
public:
    RInternalBinaryOperator op;
    RExpPtr operand0;
    RExpPtr operand1;
public:
    IR0_API RExp_CallInternalBinaryOperator(RInternalBinaryOperator op, RExpPtr&& operand0, RExpPtr&& operand1);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Call Internal

#pragma region Global

// F();
class RExp_CallGlobalFunc : public RExp
{
public:
    std::shared_ptr<RGlobalFuncDecl> funcDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    IR0_API RExp_CallGlobalFunc(const std::shared_ptr<RGlobalFuncDecl>& funcDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<RArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
    
};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
class RExp_NewClass : public RExp
{
public:
    std::shared_ptr<RClassConstructorDecl> constructorDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    IR0_API RExp_NewClass(const std::shared_ptr<RClassConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<RArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// c.F();
class RExp_CallClassMemberFunc : public RExp
{
public:
    std::shared_ptr<RClassMemberFuncDecl> classMemberFunc;
    RTypeArgumentsPtr typeArgs;
    RLocPtr instance;
    std::vector<RArgument> args;
public:
    IR0_API RExp_CallClassMemberFunc(const std::shared_ptr<RClassMemberFuncDecl>& classMemberFunc, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, std::vector<RArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// ClassStaticCast
class RExp_CastClass : public RExp
{
public:
    RExpPtr src;
    RTypePtr classType;
public:
    IR0_API RExp_CastClass(const RExpPtr& src, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
class RExp_NewStruct : public RExp
{
public:
    std::shared_ptr<RStructConstructorDecl> constructorDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    IR0_API RExp_NewStruct(const std::shared_ptr<RStructConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<RArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// s.F();
class RExp_CallStructMemberFunc : public RExp
{
public:
    std::shared_ptr<RStructMemberFuncDecl> structMemberFuncDecl;
    RTypeArgumentsPtr typeArgs;
    RLocPtr instance;
    std::vector<RArgument> args;
public:
    IR0_API RExp_CallStructMemberFunc(const std::shared_ptr<RStructMemberFuncDecl>& structMemberFuncDecl, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, std::vector<RArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
class RExp_NewEnumElem : public RExp
{
public:
    std::shared_ptr<REnumElemDecl> enumElemDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;

public:
    IR0_API RExp_NewEnumElem(const std::shared_ptr<REnumElemDecl>& enumElemDecl, const RTypeArgumentsPtr& typeArgs, std::vector<RArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 컨테이너를 enumElem -> enum으로
class RExp_CastEnumElemToEnum : public RExp
{
public:
    RExpPtr src;
    RTypePtr enumType;
public:
    IR0_API RExp_CastEnumElemToEnum(const RExpPtr& src, const RTypePtr& enumType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Enum

#pragma region Nullable

class RExp_NullableValueNullLiteral : public RExp
{
public:
    RTypePtr innerType;

public:
    IR0_API RExp_NullableValueNullLiteral(const RTypePtr& innerType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_NullableRefNullLiteral : public RExp
{
public:
    RTypePtr innerType;
public:
    IR0_API RExp_NullableRefNullLiteral(const RTypePtr& innerType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_NewNullable : public RExp
{
public:
    RExpPtr innerExp;
public:
    IR0_API RExp_NewNullable(const RExpPtr& innerExp);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
class RExp_Lambda : public RExp
{
public:
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;

public:
    IR0_API RExp_Lambda(const std::shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<RArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
class RExp_CallLambda : public RExp
{
public:
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr typeArgs;
    RLocPtr callable;
    std::vector<RArgument> args;

public:
    IR0_API RExp_CallLambda(const std::shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const RLocPtr& callable, const std::vector<RArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Lambda

#pragma region Inline

class RExp_InlineBlock : public RExp
{
public:
    std::vector<RStmtPtr> stmts;
    RTypePtr returnType;
public:
    IR0_API RExp_InlineBlock(const std::vector<RStmtPtr>& stmts, const RTypePtr& returnType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Inline

#pragma region TypeTest

class RExp_ClassIsClass : public RExp
{
public:
    RExpPtr exp;
    RTypePtr classType;

public:
    IR0_API RExp_ClassIsClass(const RExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_ClassAsClass : public RExp
{
public:
    RExpPtr exp;
    RTypePtr classType;
public:
    IR0_API RExp_ClassAsClass(const RExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
    
};

class RExp_ClassIsInterface : public RExp
{
public:
    RExpPtr exp;
    RTypePtr interfaceType; // func도 interface type이다
public:
    IR0_API RExp_ClassIsInterface(const RExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_ClassAsInterface : public RExp
{
public:
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API RExp_ClassAsInterface(const RExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_InterfaceIsClass : public RExp
{
public:
    RExpPtr exp;
    RTypePtr classType;
public:
    IR0_API RExp_InterfaceIsClass(const RExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_InterfaceAsClass : public RExp
{
public:
    RExpPtr exp;
    RTypePtr classType;
public:
    IR0_API RExp_InterfaceAsClass(const RExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_InterfaceIsInterface : public RExp
{
public:
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API RExp_InterfaceIsInterface(const RExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_InterfaceAsInterface : public RExp
{
public:
    RExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API RExp_InterfaceAsInterface(const RExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_EnumIsEnumElem : public RExp
{
public:
    RExpPtr exp;
    RTypePtr enumElemType;
public:
    IR0_API RExp_EnumIsEnumElem(const RExpPtr& exp, const RTypePtr& enumElemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

class RExp_EnumAsEnumElem : public RExp
{
public:
    RExpPtr exp;
    RTypePtr enumElemType;
public:
    IR0_API RExp_EnumAsEnumElem(const RExpPtr& exp, const RTypePtr& enumElemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(RExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion TypeTest

}