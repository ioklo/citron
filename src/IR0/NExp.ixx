export module Citron.NDecls:NExp;

import "IR0Config.h";

import <variant>;
import <memory>;
import <string>;
import <vector>;

import Citron.RDecls;

import :NArgument;

namespace Citron {

export class NExp_Load;
export class NExp_Assign;
export class NExp_Box;
export class NExp_StaticBoxRef;
export class NExp_ClassMemberBoxRef;
export class NExp_StructIndirectMemberBoxRef;
export class NExp_StructMemberBoxRef;
export class NExp_LocalRef;
export class NExp_BoolLiteral;
export class NExp_IntLiteral;
export class NExp_String;
export class NExp_List;
export class NExp_ListIterator;
export class NExp_CallInternalUnaryOperator;
export class NExp_CallInternalUnaryAssignOperator;
export class NExp_CallInternalBinaryOperator;
export class NExp_CallGlobalFunc;
export class NExp_NewClass;
export class NExp_CallClassFunc;
export class NExp_CastClass;
export class NExp_NewStruct;
export class NExp_CallStructFunc;
export class NExp_NewEnumElem;
export class NExp_CastEnumElemToEnum;
export class NExp_NewNullable;
export class NExp_NullableValueNullLiteral;
export class NExp_NullableRefNullLiteral;
export class NExp_Lambda;
export class NExp_CallLambda;
export class NExp_CastBoxedLambdaToFunc;
export class NExp_InlineBlock;
export class NExp_ClassIsClass;
export class NExp_ClassAsClass;
export class NExp_ClassIsInterface;
export class NExp_ClassAsInterface;
export class NExp_InterfaceIsClass;
export class NExp_InterfaceAsClass;
export class NExp_InterfaceIsInterface;
export class NExp_InterfaceAsInterface;
export class NExp_EnumIsEnumElem;
export class NExp_EnumAsEnumElem;

export class NLoc;
export using NLocPtr = std::shared_ptr<NLoc>;

export class RType;
export using RTypePtr = std::shared_ptr<RType>;
export class RTypeFactory;

export class NStmt;
export using NStmtPtr = std::shared_ptr<NStmt>;

export class NLambdaDecl;

export class NExpVisitor
{
public:
    virtual ~NExpVisitor() {}
    virtual void Visit(NExp_Load& exp) = 0;
    virtual void Visit(NExp_Assign& exp) = 0;
    virtual void Visit(NExp_Box& exp) = 0;
    virtual void Visit(NExp_StaticBoxRef& exp) = 0;
    virtual void Visit(NExp_ClassMemberBoxRef& exp) = 0;
    virtual void Visit(NExp_StructIndirectMemberBoxRef& exp) = 0;
    virtual void Visit(NExp_StructMemberBoxRef& exp) = 0;
    virtual void Visit(NExp_LocalRef& exp) = 0;
    virtual void Visit(NExp_BoolLiteral& exp) = 0;
    virtual void Visit(NExp_IntLiteral& exp) = 0;
    virtual void Visit(NExp_String& exp) = 0;
    virtual void Visit(NExp_List& exp) = 0;
    virtual void Visit(NExp_ListIterator& exp) = 0;
    virtual void Visit(NExp_CallInternalUnaryOperator& exp) = 0;
    virtual void Visit(NExp_CallInternalUnaryAssignOperator& exp) = 0;
    virtual void Visit(NExp_CallInternalBinaryOperator& exp) = 0;
    virtual void Visit(NExp_CallGlobalFunc& exp) = 0;
    virtual void Visit(NExp_NewClass& exp) = 0;
    virtual void Visit(NExp_CallClassFunc& exp) = 0;
    virtual void Visit(NExp_CastClass& exp) = 0;
    virtual void Visit(NExp_NewStruct& exp) = 0;
    virtual void Visit(NExp_CallStructFunc& exp) = 0;
    virtual void Visit(NExp_NewEnumElem& exp) = 0;
    virtual void Visit(NExp_CastEnumElemToEnum& exp) = 0;
    virtual void Visit(NExp_NewNullable& exp) = 0;

    virtual void Visit(NExp_NullableValueNullLiteral& exp) = 0;
    virtual void Visit(NExp_NullableRefNullLiteral& exp) = 0;

    virtual void Visit(NExp_Lambda& exp) = 0;
    virtual void Visit(NExp_CallLambda& exp) = 0;
    virtual void Visit(NExp_CastBoxedLambdaToFunc& exp) = 0;
    virtual void Visit(NExp_InlineBlock& exp) = 0;
    virtual void Visit(NExp_ClassIsClass& exp) = 0;
    virtual void Visit(NExp_ClassAsClass& exp) = 0;
    virtual void Visit(NExp_ClassIsInterface& exp) = 0;
    virtual void Visit(NExp_ClassAsInterface& exp) = 0;
    virtual void Visit(NExp_InterfaceIsClass& exp) = 0;
    virtual void Visit(NExp_InterfaceAsClass& exp) = 0;
    virtual void Visit(NExp_InterfaceIsInterface& exp) = 0;
    virtual void Visit(NExp_InterfaceAsInterface& exp) = 0;
    virtual void Visit(NExp_EnumIsEnumElem& exp) = 0;
    virtual void Visit(NExp_EnumAsEnumElem& exp) = 0;
};

export class NExp
{
public:
    virtual ~NExp() {}
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
    virtual void Accept(NExpVisitor& visitor) = 0;
};

export using NExpPtr = std::shared_ptr<NExp>;

#pragma region Storage

// Location의 Value를 resultValue에 복사한다
export class NExp_Load : public NExp
{
public:
    NLocPtr loc;

public:
    IR0_API NExp_Load(NLocPtr&& loc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// a = b
export class NExp_Assign : public NExp
{
public:
    NLocPtr dest;
    NExpPtr src;
public:
    IR0_API NExp_Assign(NLocPtr&& dest, NExpPtr&& src);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box 3
export class NExp_Box : public NExp
{
public:
    NExpPtr innerExp;
public:
    IR0_API NExp_Box(NExpPtr&& innerExp);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &C.x
export class NExp_StaticBoxRef : public NExp
{
public:
    NLocPtr loc;
public:
    IR0_API NExp_StaticBoxRef(const NLocPtr& loc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
export class NExp_ClassMemberBoxRef : public NExp
{
public:
    NLocPtr holder;
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NExp_ClassMemberBoxRef(const NLocPtr& holder, const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
export class NExp_StructIndirectMemberBoxRef : public NExp
{
public:
    NLocPtr holder;
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NExp_StructIndirectMemberBoxRef(const NLocPtr& holder, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
export class NExp_StructMemberBoxRef : public NExp
{
public:
    NLocPtr parent;
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NExp_StructMemberBoxRef(const NLocPtr& parent, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// &i
export class NExp_LocalRef : public NExp
{
public:
    NLocPtr innerLoc;

public:
    IR0_API NExp_LocalRef(const NLocPtr& innerLoc);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Storage

#pragma region Interface

// func<int, int> f = box (int x) => x + p;
export class NExp_CastBoxedLambdaToFunc : public NExp
{
public:
    NExpPtr exp;
    std::shared_ptr<RType_Func> funcType;
public:
    IR0_API NExp_CastBoxedLambdaToFunc(const NExpPtr& exp, const std::shared_ptr<RType_Func>& funcType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Interface

#pragma region Literal

// false
export class NExp_BoolLiteral : public NExp
{
public:
    bool value;
public:
    IR0_API NExp_BoolLiteral(bool value);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 1
export class NExp_IntLiteral : public NExp
{
public:
    int value;
public:
    IR0_API NExp_IntLiteral(int value);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class RTextStringExpElement
{
public:
    std::string text;

public:
    IR0_API RTextStringExpElement(const std::string& text);
};

export class RLocStringExpElement
{
public:
    NLocPtr loc;

public:
    IR0_API RLocStringExpElement(NLocPtr&& loc);
};

export using RStringExpElement = std::variant<RTextStringExpElement, RLocStringExpElement>;

// "dskfjslkf $abc "
export class NExp_String : public NExp
{
public:
    std::vector<RStringExpElement> elements;

public:
    IR0_API NExp_String(std::vector<RStringExpElement>&& elements);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }

};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
export class NExp_List : public NExp
{
public:
    std::vector<NExpPtr> elems;
    RTypePtr itemType;
public:
    IR0_API NExp_List(std::vector<NExpPtr>&& elems, const RTypePtr& itemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_ListIterator : public NExp
{
public:
    NLocPtr listLoc;
    RTypePtr type;
public:
    IR0_API NExp_ListIterator(const NLocPtr& listLoc, const RTypePtr& type);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion List

#pragma region Call Internal

export enum class RInternalUnaryOperator
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    ToString_Bool_String,
    ToString_Int_String,
};

export enum class RInternalUnaryAssignOperator
{
    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,
};

export enum class RInternalBinaryOperator
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

export class NExp_CallInternalUnaryOperator : public NExp
{
public:
    RInternalUnaryOperator op;
    NExpPtr operand;
public:
    IR0_API NExp_CallInternalUnaryOperator(RInternalUnaryOperator op, NExpPtr&& operand);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_CallInternalUnaryAssignOperator : public NExp
{
public:
    RInternalUnaryAssignOperator op;
    NLocPtr operand;
public:
    IR0_API NExp_CallInternalUnaryAssignOperator(RInternalUnaryAssignOperator op, NLocPtr&& operand);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_CallInternalBinaryOperator : public NExp
{
public:
    RInternalBinaryOperator op;
    NExpPtr operand0;
    NExpPtr operand1;
public:
    IR0_API NExp_CallInternalBinaryOperator(RInternalBinaryOperator op, NExpPtr&& operand0, NExpPtr&& operand1);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Call Internal

#pragma region Global

// F();
export class NExp_CallGlobalFunc : public NExp
{
public:
    std::shared_ptr<RGlobalFuncDecl> funcDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallGlobalFunc(const std::shared_ptr<RGlobalFuncDecl>& funcDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<NArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }

};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
export class NExp_NewClass : public NExp
{
public:
    std::shared_ptr<RClassCtorDecl> ctorDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_NewClass(const std::shared_ptr<RClassCtorDecl>& ctorDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<NArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// c.F();
export class NExp_CallClassFunc : public NExp
{
public:
    std::shared_ptr<RClassFuncDecl> decl;
    RTypeArgumentsPtr typeArgs;
    NLocPtr instance;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallClassFunc(std::shared_ptr<RClassFuncDecl>&& decl, RTypeArgumentsPtr&& typeArgs, NLocPtr&& instance, std::vector<NArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// ClassStaticCast
export class NExp_CastClass : public NExp
{
public:
    NExpPtr src;
    RTypePtr classType;
public:
    IR0_API NExp_CastClass(const NExpPtr& src, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
export class NExp_NewStruct : public NExp
{
public:
    std::shared_ptr<RStructCtorDecl> ctor;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_NewStruct(const std::shared_ptr<RStructCtorDecl>& ctor, RTypeArgumentsPtr&& typeArgs, std::vector<NArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// s.F();
export class NExp_CallStructFunc : public NExp
{
public:
    std::shared_ptr<RStructFuncDecl> decl;
    RTypeArgumentsPtr typeArgs;
    NLocPtr instance;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallStructFunc(std::shared_ptr<RStructFuncDecl>&& decl, RTypeArgumentsPtr&& typeArgs, NLocPtr&& instance, std::vector<NArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
export class NExp_NewEnumElem : public NExp
{
public:
    std::shared_ptr<REnumElemDecl> enumElemDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;

public:
    IR0_API NExp_NewEnumElem(const std::shared_ptr<REnumElemDecl>& enumElemDecl, const RTypeArgumentsPtr& typeArgs, std::vector<NArgument>&& args);
    IR0_API NExp_NewEnumElem(const std::shared_ptr<REnumElemDecl>& enumElemDecl, RTypeArgumentsPtr&& typeArgs, std::vector<NArgument>&& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 컨테이너를 enumElem -> enum으로
export class NExp_CastEnumElemToEnum : public NExp
{
public:
    NExpPtr src;
    RTypePtr enumType;
public:
    IR0_API NExp_CastEnumElemToEnum(const NExpPtr& src, const RTypePtr& enumType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Enum

#pragma region Nullable

export class NExp_NullableValueNullLiteral : public NExp
{
public:
    RTypePtr innerType;

public:
    IR0_API NExp_NullableValueNullLiteral(const RTypePtr& innerType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_NullableRefNullLiteral : public NExp
{
public:
    RTypePtr innerType;
public:
    IR0_API NExp_NullableRefNullLiteral(const RTypePtr& innerType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_NewNullable : public NExp
{
public:
    NExpPtr innerExp;
public:
    IR0_API NExp_NewNullable(const NExpPtr& innerExp);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
export class NExp_Lambda : public NExp
{
public:
    std::shared_ptr<NLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;

public:
    IR0_API NExp_Lambda(const std::shared_ptr<NLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const std::vector<NArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
export class NExp_CallLambda : public NExp
{
public:
    // TODO: RType_Lambda에 있는 정보들, callable->GetType()하면 얻을수 있는 것들이다. 삭제해야 하지 않을까
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr typeArgs;

    NLocPtr callable;
    std::vector<NArgument> args;

public:
    IR0_API NExp_CallLambda(const std::shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const NLocPtr& callable, const std::vector<NArgument>& args);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Lambda

#pragma region Inline

export class NExp_InlineBlock : public NExp
{
public:
    std::vector<NStmtPtr> stmts;
    RTypePtr returnType;
public:
    IR0_API NExp_InlineBlock(const std::vector<NStmtPtr>& stmts, const RTypePtr& returnType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion Inline

#pragma region TypeTest

export class NExp_ClassIsClass : public NExp
{
public:
    NExpPtr exp;
    RTypePtr classType;

public:
    IR0_API NExp_ClassIsClass(NExpPtr&& exp, RTypePtr&& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_ClassAsClass: public NExp
{
public:
    NExpPtr exp;
    RTypePtr classType;
public:
    IR0_API NExp_ClassAsClass(const NExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }

};

export class NExp_ClassIsInterface : public NExp
{
public:
    NExpPtr exp;
    RTypePtr interfaceType; // func도 interface type이다
public:
    IR0_API NExp_ClassIsInterface(NExpPtr&& exp, RTypePtr&& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_ClassAsInterface : public NExp
{
public:
    NExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API NExp_ClassAsInterface(const NExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_InterfaceIsClass : public NExp
{
public:
    NExpPtr exp;
    RTypePtr classType;
public:
    IR0_API NExp_InterfaceIsClass(NExpPtr&& exp, RTypePtr&& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_InterfaceAsClass : public NExp
{
public:
    NExpPtr exp;
    RTypePtr classType;
public:
    IR0_API NExp_InterfaceAsClass(const NExpPtr& exp, const RTypePtr& classType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_InterfaceIsInterface : public NExp
{
public:
    NExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API NExp_InterfaceIsInterface(NExpPtr&& exp, RTypePtr&& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_InterfaceAsInterface : public NExp
{
public:
    NExpPtr exp;
    RTypePtr interfaceType;
public:
    IR0_API NExp_InterfaceAsInterface(const NExpPtr& exp, const RTypePtr& interfaceType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_EnumIsEnumElem : public NExp
{
public:
    NExpPtr exp;
    RTypePtr enumElemType;
public:
    IR0_API NExp_EnumIsEnumElem(NExpPtr&& exp, RTypePtr&& enumElemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class NExp_EnumAsEnumElem : public NExp
{
public:
    NExpPtr exp;
    RTypePtr enumElemType;
public:
    IR0_API NExp_EnumAsEnumElem(const NExpPtr& exp, const RTypePtr& enumElemType);

    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(*this); }
};

#pragma endregion TypeTest

}