#pragma once
#include "IR0Config.h"

#include <variant>
#include <string>
#include <vector>
#include <optional>

#include "NArgument.h"

namespace Citron {

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RClassVarDecl;
class RStructCtorDecl;
class RStructFuncDecl;
class RStructVarDecl;

class REnumElemDecl;

class RLambdaDecl;

class RTypeArguments;
class RType_Func;

class NExp_Load;
class NExp_Assign;
class NExp_Box;
class NExp_StaticBoxRef;
class NExp_ClassMemberBoxRef;
class NExp_StructIndirectMemberBoxRef;
class NExp_StructMemberBoxRef;
class NExp_LocalRef;
class NExp_BoolLiteral;
class NExp_IntLiteral;
class NExp_String;
class NExp_List;
class NExp_ListIterator;
class NExp_CallInternalUnaryOperator;
class NExp_CallInternalUnaryAssignOperator;
class NExp_CallInternalBinaryOperator;
class NExp_CallGlobalFunc;
class NExp_NewClass;
class NExp_CallClassFunc;
class NExp_CastClass;
class NExp_NewStruct;
class NExp_CallStructFunc;
class NExp_NewEnumElem;
class NExp_CastEnumElemToEnum;
class NExp_NewNullable;
class NExp_NullableValueNullLiteral;
class NExp_NullableRefNullLiteral;
class NExp_Lambda;
class NExp_CallLambda;
class NExp_CastBoxedLambdaToFunc;
class NExp_InlineBlock;
class NExp_ClassIsClass;
class NExp_ClassAsClass;
class NExp_ClassIsInterface;
class NExp_ClassAsInterface;
class NExp_InterfaceIsClass;
class NExp_InterfaceAsClass;
class NExp_InterfaceIsInterface;
class NExp_InterfaceAsInterface;
class NExp_EnumIsEnumElem;
class NExp_EnumAsEnumElem;

class NLoc;

class RType;
class RFactory;

class NStmt;
class NLambdaDecl;

class NExpVisitor
{
public:
    virtual ~NExpVisitor() {}
    virtual void Visit(NExp_Load* exp) = 0;
    virtual void Visit(NExp_Assign* exp) = 0;
    virtual void Visit(NExp_Box* exp) = 0;
    virtual void Visit(NExp_StaticBoxRef* exp) = 0;
    virtual void Visit(NExp_ClassMemberBoxRef* exp) = 0;
    virtual void Visit(NExp_StructIndirectMemberBoxRef* exp) = 0;
    virtual void Visit(NExp_StructMemberBoxRef* exp) = 0;
    virtual void Visit(NExp_LocalRef* exp) = 0;
    virtual void Visit(NExp_BoolLiteral* exp) = 0;
    virtual void Visit(NExp_IntLiteral* exp) = 0;
    virtual void Visit(NExp_String* exp) = 0;
    virtual void Visit(NExp_List* exp) = 0;
    virtual void Visit(NExp_ListIterator* exp) = 0;
    virtual void Visit(NExp_CallInternalUnaryOperator* exp) = 0;
    virtual void Visit(NExp_CallInternalUnaryAssignOperator* exp) = 0;
    virtual void Visit(NExp_CallInternalBinaryOperator* exp) = 0;
    virtual void Visit(NExp_CallGlobalFunc* exp) = 0;
    virtual void Visit(NExp_NewClass* exp) = 0;
    virtual void Visit(NExp_CallClassFunc* exp) = 0;
    virtual void Visit(NExp_CastClass* exp) = 0;
    virtual void Visit(NExp_NewStruct* exp) = 0;
    virtual void Visit(NExp_CallStructFunc* exp) = 0;
    virtual void Visit(NExp_NewEnumElem* exp) = 0;
    virtual void Visit(NExp_CastEnumElemToEnum* exp) = 0;
    virtual void Visit(NExp_NewNullable* exp) = 0;

    virtual void Visit(NExp_NullableValueNullLiteral* exp) = 0;
    virtual void Visit(NExp_NullableRefNullLiteral* exp) = 0;

    virtual void Visit(NExp_Lambda* exp) = 0;
    virtual void Visit(NExp_CallLambda* exp) = 0;
    virtual void Visit(NExp_CastBoxedLambdaToFunc* exp) = 0;
    virtual void Visit(NExp_InlineBlock* exp) = 0;
    virtual void Visit(NExp_ClassIsClass* exp) = 0;
    virtual void Visit(NExp_ClassAsClass* exp) = 0;
    virtual void Visit(NExp_ClassIsInterface* exp) = 0;
    virtual void Visit(NExp_ClassAsInterface* exp) = 0;
    virtual void Visit(NExp_InterfaceIsClass* exp) = 0;
    virtual void Visit(NExp_InterfaceAsClass* exp) = 0;
    virtual void Visit(NExp_InterfaceIsInterface* exp) = 0;
    virtual void Visit(NExp_InterfaceAsInterface* exp) = 0;
    virtual void Visit(NExp_EnumIsEnumElem* exp) = 0;
    virtual void Visit(NExp_EnumAsEnumElem* exp) = 0;
};

class NExp
{
public:
    virtual ~NExp() {}
    virtual RType* GetType(RFactory& factory) = 0;
    virtual void Accept(NExpVisitor& visitor) = 0;
};

#pragma region Storage

// Location의 Value를 resultValue에 복사한다
class NExp_Load : public NExp
{
public:
    NLoc* loc;

public:
    IR0_API NExp_Load(NLoc* loc);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// a = b
class NExp_Assign : public NExp
{
public:
    NLoc* dest;
    NExp* src;
public:
    IR0_API NExp_Assign(NLoc* dest, NExp* src);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// box 3
class NExp_Box : public NExp
{
public:
    NExp* innerExp;
public:
    IR0_API NExp_Box(NExp* innerExp);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// &C.x
class NExp_StaticBoxRef : public NExp
{
public:
    NLoc* loc;
public:
    IR0_API NExp_StaticBoxRef(NLoc* loc);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
class NExp_ClassMemberBoxRef : public NExp
{
public:
    NLoc* holder;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NExp_ClassMemberBoxRef(NLoc* holder, RClassVarDecl* decl, RTypeArguments* typeArgs);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
class NExp_StructIndirectMemberBoxRef : public NExp
{
public:
    NLoc* holder;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NExp_StructIndirectMemberBoxRef(NLoc* holder, RStructVarDecl* decl, RTypeArguments* typeArgs);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
class NExp_StructMemberBoxRef : public NExp
{
public:
    NLoc* parent;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NExp_StructMemberBoxRef(NLoc* parent, RStructVarDecl* decl, RTypeArguments* typeArgs);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// &i
class NExp_LocalRef : public NExp
{
public:
    NLoc* innerLoc;

public:
    IR0_API NExp_LocalRef(NLoc* innerLoc);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Storage

#pragma region Interface

// func<int, int> f = box (int x) => x + p;
class NExp_CastBoxedLambdaToFunc : public NExp
{
public:
    NExp* exp;
    RType_Func* funcType;
public:
    IR0_API NExp_CastBoxedLambdaToFunc(NExp* exp, RType_Func* funcType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Interface

#pragma region Literal

// false
class NExp_BoolLiteral : public NExp
{
public:
    bool value;
public:
    IR0_API NExp_BoolLiteral(bool value);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// 1
class NExp_IntLiteral : public NExp
{
public:
    int value;
public:
    IR0_API NExp_IntLiteral(int value);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NTextStringExpElement
{
public:
    std::string text;

public:
    IR0_API NTextStringExpElement(const std::string& text);
};

class NLocStringExpElement
{
public:
    NLoc* loc;

public:
    IR0_API NLocStringExpElement(NLoc* loc);
};

using NStringExpElement = std::variant<NTextStringExpElement, NLocStringExpElement>;

// "dskfjslkf $abc "
class NExp_String : public NExp
{
public:
    std::vector<NStringExpElement> elements;

public:
    IR0_API NExp_String(std::vector<NStringExpElement>&& elements);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }

};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
class NExp_List : public NExp
{
public:
    std::vector<NExp*> elems;
    RType* itemType;
public:
    IR0_API NExp_List(std::vector<NExp*>&& elems, RType* itemType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_ListIterator : public NExp
{
public:
    NLoc* listLoc;
    RType* type;
public:
    IR0_API NExp_ListIterator(NLoc* listLoc, RType* type);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion List

#pragma region Call Internal

enum class NInternalUnaryOperator
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    ToString_Bool_String,
    ToString_Int_String,
};

enum class NInternalUnaryAssignOperator
{
    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,
};

enum class NInternalBinaryOperator
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

class NExp_CallInternalUnaryOperator : public NExp
{
public:
    NInternalUnaryOperator op;
    NExp* operand;
public:
    IR0_API NExp_CallInternalUnaryOperator(NInternalUnaryOperator op, NExp* operand);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_CallInternalUnaryAssignOperator : public NExp
{
public:
    NInternalUnaryAssignOperator op;
    NLoc* operand;
public:
    IR0_API NExp_CallInternalUnaryAssignOperator(NInternalUnaryAssignOperator op, NLoc* operand);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_CallInternalBinaryOperator : public NExp
{
public:
    NInternalBinaryOperator op;
    NExp* operand0;
    NExp* operand1;
public:
    IR0_API NExp_CallInternalBinaryOperator(NInternalBinaryOperator op, NExp* operand0, NExp* operand1);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Call Internal

#pragma region Global

// F();
class NExp_CallGlobalFunc : public NExp
{
public:
    RGlobalFuncDecl* funcDecl;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallGlobalFunc(RGlobalFuncDecl* funcDecl, RTypeArguments* typeArgs, const std::vector<NArgument>& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }

};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
class NExp_NewClass : public NExp
{
public:
    RClassCtorDecl* ctorDecl;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_NewClass(RClassCtorDecl* ctorDecl, RTypeArguments* typeArgs, const std::vector<NArgument>& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// c.F();
class NExp_CallClassFunc : public NExp
{
public:
    RClassFuncDecl* decl;
    RTypeArguments* typeArgs;
    NLoc* instance;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallClassFunc(RClassFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, std::vector<NArgument>&& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// ClassStaticCast
class NExp_CastClass : public NExp
{
public:
    NExp* src;
    RType* classType;
public:
    IR0_API NExp_CastClass(NExp* src, RType* classType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
class NExp_NewStruct : public NExp
{
public:
    RStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, std::vector<NArgument>&& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// s.F();
class NExp_CallStructFunc : public NExp
{
public:
    RStructFuncDecl* decl;
    RTypeArguments* typeArgs;
    NLoc* instance;
    std::vector<NArgument> args;
public:
    IR0_API NExp_CallStructFunc(RStructFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, std::vector<NArgument>&& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
class NExp_NewEnumElem : public NExp
{
public:
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;

public:
    IR0_API NExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, std::vector<NArgument>&& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// 컨테이너를 enumElem -> enum으로
class NExp_CastEnumElemToEnum : public NExp
{
public:
    NExp* src;
    RType* enumType;
public:
    IR0_API NExp_CastEnumElemToEnum(NExp* src, RType* enumType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Enum

#pragma region Nullable

class NExp_NullableValueNullLiteral : public NExp
{
public:
    RType* innerType;

public:
    IR0_API NExp_NullableValueNullLiteral(RType* innerType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_NullableRefNullLiteral : public NExp
{
public:
    RType* innerType;
public:
    IR0_API NExp_NullableRefNullLiteral(RType* innerType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_NewNullable : public NExp
{
public:
    NExp* innerExp;
public:
    IR0_API NExp_NewNullable(NExp* innerExp);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
class NExp_Lambda : public NExp
{
public:
    NLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;

public:
    IR0_API NExp_Lambda(NLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, const std::vector<NArgument>& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
class NExp_CallLambda : public NExp
{
public:
    // TODO: RType_Lambda에 있는 정보들, callable->GetType()하면 얻을수 있는 것들이다. 삭제해야 하지 않을까
    RLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;

    NLoc* callable;
    std::vector<NArgument> args;

public:
    IR0_API NExp_CallLambda(RLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, NLoc* callable, const std::vector<NArgument>& args);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Lambda

#pragma region Inline

class NExp_InlineBlock : public NExp
{
public:
    std::vector<NStmt*> stmts;
    RType* returnType;
public:
    IR0_API NExp_InlineBlock(const std::vector<NStmt*>& stmts, RType* returnType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Inline

#pragma region TypeTest

class NExp_ClassIsClass : public NExp
{
public:
    NExp* exp;
    RType* classType;

public:
    IR0_API NExp_ClassIsClass(NExp* exp, RType* classType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_ClassAsClass: public NExp
{
public:
    NExp* exp;
    RType* classType;
public:
    IR0_API NExp_ClassAsClass(NExp* exp, RType* classType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }

};

class NExp_ClassIsInterface : public NExp
{
public:
    NExp* exp;
    RType* interfaceType; // func도 interface type이다
public:
    IR0_API NExp_ClassIsInterface(NExp* exp, RType* interfaceType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_ClassAsInterface : public NExp
{
public:
    NExp* exp;
    RType* interfaceType;
public:
    IR0_API NExp_ClassAsInterface(NExp* exp, RType* interfaceType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_InterfaceIsClass : public NExp
{
public:
    NExp* exp;
    RType* classType;
public:
    IR0_API NExp_InterfaceIsClass(NExp* exp, RType* classType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_InterfaceAsClass : public NExp
{
public:
    NExp* exp;
    RType* classType;
public:
    IR0_API NExp_InterfaceAsClass(NExp* exp, RType* classType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_InterfaceIsInterface : public NExp
{
public:
    NExp* exp;
    RType* interfaceType;
public:
    IR0_API NExp_InterfaceIsInterface(NExp* exp, RType* interfaceType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_InterfaceAsInterface : public NExp
{
public:
    NExp* exp;
    RType* interfaceType;
public:
    IR0_API NExp_InterfaceAsInterface(NExp* exp, RType* interfaceType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_EnumIsEnumElem : public NExp
{
public:
    NExp* exp;
    RType* enumElemType;
public:
    IR0_API NExp_EnumIsEnumElem(NExp* exp, RType* enumElemType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

class NExp_EnumAsEnumElem : public NExp
{
public:
    NExp* exp;
    RType* enumElemType;
public:
    IR0_API NExp_EnumAsEnumElem(NExp* exp, RType* enumElemType);

    IR0_API RType* GetType(RFactory& factory) override;
    void Accept(NExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion TypeTest

template<class TFrom, class TVisitor>
concept NExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept NExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<NExp_Load*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_Assign*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_Box*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_StaticBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ClassMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_StructIndirectMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_StructMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_LocalRef*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_BoolLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_IntLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_String*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_List*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ListIterator*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallInternalUnaryOperator*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallInternalUnaryAssignOperator*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallInternalBinaryOperator*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallGlobalFunc*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NewClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallClassFunc*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CastClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NewStruct*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallStructFunc*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NewEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CastEnumElemToEnum*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NewNullable*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NullableValueNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_NullableRefNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CallLambda*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_CastBoxedLambdaToFunc*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_InlineBlock*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ClassIsClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ClassAsClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ClassIsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_ClassAsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_InterfaceIsClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_InterfaceAsClass*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_InterfaceIsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_InterfaceAsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_EnumIsEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NExp_EnumAsEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> NExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires NExpVisitable<TVisitor, TVisitorArgs...>
decltype(auto) Accept(TVisitor&& v, NExp* nExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : NExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(NExp_Load* nExp) override { call(nExp); }
            void Visit(NExp_Assign* nExp) override { call(nExp); }
            void Visit(NExp_Box* nExp) override { call(nExp); }
            void Visit(NExp_StaticBoxRef* nExp) override { call(nExp); }
            void Visit(NExp_ClassMemberBoxRef* nExp) override { call(nExp); }
            void Visit(NExp_StructIndirectMemberBoxRef* nExp) override { call(nExp); }
            void Visit(NExp_StructMemberBoxRef* nExp) override { call(nExp); }
            void Visit(NExp_LocalRef* nExp) override { call(nExp); }
            void Visit(NExp_BoolLiteral* nExp) override { call(nExp); }
            void Visit(NExp_IntLiteral* nExp) override { call(nExp); }
            void Visit(NExp_String* nExp) override { call(nExp); }
            void Visit(NExp_List* nExp) override { call(nExp); }
            void Visit(NExp_ListIterator* nExp) override { call(nExp); }
            void Visit(NExp_CallInternalUnaryOperator* nExp) override { call(nExp); }
            void Visit(NExp_CallInternalUnaryAssignOperator* nExp) override { call(nExp); }
            void Visit(NExp_CallInternalBinaryOperator* nExp) override { call(nExp); }
            void Visit(NExp_CallGlobalFunc* nExp) override { call(nExp); }
            void Visit(NExp_NewClass* nExp) override { call(nExp); }
            void Visit(NExp_CallClassFunc* nExp) override { call(nExp); }
            void Visit(NExp_CastClass* nExp) override { call(nExp); }
            void Visit(NExp_NewStruct* nExp) override { call(nExp); }
            void Visit(NExp_CallStructFunc* nExp) override { call(nExp); }
            void Visit(NExp_NewEnumElem* nExp) override { call(nExp); }
            void Visit(NExp_CastEnumElemToEnum* nExp) override { call(nExp); }
            void Visit(NExp_NewNullable* nExp) override { call(nExp); }
            void Visit(NExp_NullableValueNullLiteral* nExp) override { call(nExp); }
            void Visit(NExp_NullableRefNullLiteral* nExp) override { call(nExp); }
            void Visit(NExp_Lambda* nExp) override { call(nExp); }
            void Visit(NExp_CallLambda* nExp) override { call(nExp); }
            void Visit(NExp_CastBoxedLambdaToFunc* nExp) override { call(nExp); }
            void Visit(NExp_InlineBlock* nExp) override { call(nExp); }
            void Visit(NExp_ClassIsClass* nExp) override { call(nExp); }
            void Visit(NExp_ClassAsClass* nExp) override { call(nExp); }
            void Visit(NExp_ClassIsInterface* nExp) override { call(nExp); }
            void Visit(NExp_ClassAsInterface* nExp) override { call(nExp); }
            void Visit(NExp_InterfaceIsClass* nExp) override { call(nExp); }
            void Visit(NExp_InterfaceAsClass* nExp) override { call(nExp); }
            void Visit(NExp_InterfaceIsInterface* nExp) override { call(nExp); }
            void Visit(NExp_InterfaceAsInterface* nExp) override { call(nExp); }
            void Visit(NExp_EnumIsEnumElem* nExp) override { call(nExp); }
            void Visit(NExp_EnumAsEnumElem* nExp) override { call(nExp); }
        };

        Bridge bridge{caller};
        nExp->Accept(bridge);
    }
    else
    {
        struct Bridge : NExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(NExp_Load* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_Assign* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_Box* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_StaticBoxRef* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ClassMemberBoxRef* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_StructIndirectMemberBoxRef* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_StructMemberBoxRef* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_LocalRef* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_BoolLiteral* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_IntLiteral* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_String* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_List* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ListIterator* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallInternalUnaryOperator* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallInternalUnaryAssignOperator* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallInternalBinaryOperator* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallGlobalFunc* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NewClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallClassFunc* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CastClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NewStruct* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallStructFunc* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NewEnumElem* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CastEnumElemToEnum* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NewNullable* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NullableValueNullLiteral* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_NullableRefNullLiteral* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_Lambda* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CallLambda* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_CastBoxedLambdaToFunc* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_InlineBlock* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ClassIsClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ClassAsClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ClassIsInterface* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_ClassAsInterface* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_InterfaceIsClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_InterfaceAsClass* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_InterfaceIsInterface* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_InterfaceAsInterface* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_EnumIsEnumElem* nExp) override { result.emplace(call(nExp)); }
            void Visit(NExp_EnumAsEnumElem* nExp) override { result.emplace(call(nExp)); }
        };

        Bridge bridge{caller};
        nExp->Accept(bridge);
        return *bridge.result;
    }
}

}