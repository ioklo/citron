#pragma once
#include "MIRConfig.h"

#include <variant>
#include <string>
#include <vector>
#include <optional>
#include <memory>

#include "MCreate.h"
#include "MArgument.h"

namespace Citron {

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RClassVarDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RStructVarDecl;

class REnumElemDecl;

class RLambdaDecl;

class RTypeArguments;
class RType_Func;
class RType_EnumElem;

class MExp_BitwiseCopy;
class MExp_BitwiseAssign;
class MExp_Stmt;
class MExp_Shared;
class MExp_StaticBoxRef;
class MExp_ClassMemberBoxRef;
class MExp_StructIndirectMemberBoxRef;
class MExp_StructMemberBoxRef;
class MExp_PtrRef;
class MExp_BoolLiteral;
class MExp_IntLiteral;
class MExp_String;
class MExp_List;
class MExp_ListIterator;
class MExp_CallInternalUnaryOperator;
class MExp_CallInternalUnaryAssignOperator;
class MExp_CallInternalBinaryOperator;
class MExp_CallGlobalFunc;
class MExp_NewClass;
class MExp_CallClassFunc;
class MExp_CastClass;
class MExp_NewStruct;
class MExp_CallStructFunc;
class MExp_NewEnumElem;
class MExp_CastEnumElemToEnum;
class MExp_NewNullable;
class MExp_NullableValueNullLiteral;
class MExp_NullableRefNullLiteral;
class MExp_Lambda;
class MExp_CallLambda;
class MExp_CastBoxedLambdaToFunc;
class MExp_InlineBlock;
class MExp_ClassIsClass;
class MExp_ClassAsClass;
class MExp_ClassIsInterface;
class MExp_ClassAsInterface;
class MExp_InterfaceIsClass;
class MExp_InterfaceAsClass;
class MExp_InterfaceIsInterface;
class MExp_InterfaceAsInterface;
class MExp_EnumIsEnumElem;
class MExp_EnumAsEnumElem;

class MLoc;

class RType;
class RFactory;

class MStmt;
class NLambdaDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class MExpVisitor
{
public:
    virtual ~MExpVisitor() {}
    virtual void Visit(MExp_BitwiseCopy* exp) = 0;
    virtual void Visit(MExp_BitwiseAssign* exp) = 0;
    virtual void Visit(MExp_Stmt* exp) = 0;
    virtual void Visit(MExp_Shared* exp) = 0;
    virtual void Visit(MExp_StaticBoxRef* exp) = 0;
    virtual void Visit(MExp_ClassMemberBoxRef* exp) = 0;
    virtual void Visit(MExp_StructIndirectMemberBoxRef* exp) = 0;
    virtual void Visit(MExp_StructMemberBoxRef* exp) = 0;
    virtual void Visit(MExp_PtrRef* exp) = 0;
    virtual void Visit(MExp_BoolLiteral* exp) = 0;
    virtual void Visit(MExp_IntLiteral* exp) = 0;
    virtual void Visit(MExp_String* exp) = 0;
    virtual void Visit(MExp_List* exp) = 0;
    virtual void Visit(MExp_ListIterator* exp) = 0;
    virtual void Visit(MExp_CallInternalUnaryOperator* exp) = 0;
    virtual void Visit(MExp_CallInternalUnaryAssignOperator* exp) = 0;
    virtual void Visit(MExp_CallInternalBinaryOperator* exp) = 0;
    virtual void Visit(MExp_CallGlobalFunc* exp) = 0;
    virtual void Visit(MExp_NewClass* exp) = 0;
    virtual void Visit(MExp_CallClassFunc* exp) = 0;
    virtual void Visit(MExp_CastClass* exp) = 0;
    virtual void Visit(MExp_NewStruct* exp) = 0;
    virtual void Visit(MExp_CallStructFunc* exp) = 0;
    virtual void Visit(MExp_NewEnumElem* exp) = 0;
    virtual void Visit(MExp_CastEnumElemToEnum* exp) = 0;
    virtual void Visit(MExp_NewNullable* exp) = 0;

    virtual void Visit(MExp_NullableValueNullLiteral* exp) = 0;
    virtual void Visit(MExp_NullableRefNullLiteral* exp) = 0;

    virtual void Visit(MExp_Lambda* exp) = 0;
    virtual void Visit(MExp_CallLambda* exp) = 0;
    virtual void Visit(MExp_CastBoxedLambdaToFunc* exp) = 0;
    virtual void Visit(MExp_InlineBlock* exp) = 0;
    virtual void Visit(MExp_ClassIsClass* exp) = 0;
    virtual void Visit(MExp_ClassAsClass* exp) = 0;
    virtual void Visit(MExp_ClassIsInterface* exp) = 0;
    virtual void Visit(MExp_ClassAsInterface* exp) = 0;
    virtual void Visit(MExp_InterfaceIsClass* exp) = 0;
    virtual void Visit(MExp_InterfaceAsClass* exp) = 0;
    virtual void Visit(MExp_InterfaceIsInterface* exp) = 0;
    virtual void Visit(MExp_InterfaceAsInterface* exp) = 0;
    virtual void Visit(MExp_EnumIsEnumElem* exp) = 0;
    virtual void Visit(MExp_EnumAsEnumElem* exp) = 0;
};

class MExp
{
public:
    virtual ~MExp() {}
    virtual RType* GetType() = 0;
    virtual void Accept(MExpVisitor& visitor) = 0;
};

#pragma region Storage

// 주어진 Location의 값을 비트단위로 복사한다
class MExp_BitwiseCopy : public MExp
{
public:
    MLoc* loc;
public:
    MExp_BitwiseCopy(MLoc* loc)
        : loc{loc}
    {}

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// 주어진 Exp를 값으로 계산해서 Location에 비트단위로 복사하고, 그 값을 나타낸다
class MExp_BitwiseAssign : public MExp
{
public:
    MLoc* dest;
    MExp* src;
public:
    MExp_BitwiseAssign(MLoc* dest, MExp* src)
        : dest{dest}, src{src}
    {}
    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// { stmt1; stmt2; ...; finalExp; }
class MExp_Stmt : public MExp
{
public:
    std::vector<MStmt*> stmts;
    MExp* finalExp;

public:
    MIR_API MExp_Stmt(std::vector<MStmt*>&& stmts, MExp* finalExp);
    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// box int i = box 3
class MExp_Shared : public MExp
{
public:
    MCreate innerCreate;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_Shared(MCreate&& innerCreate, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// &C.x
class MExp_StaticBoxRef : public MExp
{
    RFactoryPtr rFactory;
public:
    MLoc* loc;
public:
    MIR_API MExp_StaticBoxRef(MLoc* loc, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// &c.x => RClassMemberBoxRefExp(RLocalVar("c"), C::x)
class MExp_ClassMemberBoxRef : public MExp
{
    RFactoryPtr rFactory;
public:
    MLoc* holder;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MExp_ClassMemberBoxRef(MLoc* holder, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// box S* pS;
// &ps->x => RStructIndirectMemberBoxRefExp(RLocalVar("pS"), S::x)
class MExp_StructIndirectMemberBoxRef : public MExp
{
    RFactoryPtr rFactory;
public:
    MLoc* holder;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MExp_StructIndirectMemberBoxRef(MLoc* holder, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// C c;
// box A* a = &c.s.a; => RStructMemberBoxRefExp(RClassMemberBoxRefExp(RLocalVar("c"), C::s), A::a)
class MExp_StructMemberBoxRef : public MExp
{
public:
    MLoc* parent;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_StructMemberBoxRef(MLoc* parent, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// &i
class MExp_PtrRef : public MExp
{
public:
    MLoc* innerLoc;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_PtrRef(MLoc* innerLoc, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Storage

#pragma region Interface

// func<int, int> f = box (int x) => x + p;
class MExp_CastBoxedLambdaToFunc : public MExp
{
public:
    MExp* exp;
    RType_Func* funcType;
public:
    MIR_API MExp_CastBoxedLambdaToFunc(MExp* exp, RType_Func* funcType);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Interface

#pragma region Literal

// false
class MExp_BoolLiteral : public MExp
{
public:
    bool value;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_BoolLiteral(bool value, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// 1
class MExp_IntLiteral : public MExp
{
public:
    int value;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_IntLiteral(int value, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_StringElem_Text
{
public:
    std::string text;

public:
    MIR_API MExp_StringElem_Text(const std::string& text);
};

class MExp_StringElem_Exp
{
public:
    MExp* mExp;

public:
    MIR_API MExp_StringElem_Exp(MExp* mExp);
};

using MExp_StringElem = std::variant<MExp_StringElem_Text, MExp_StringElem_Exp>;

// "dskfjslkf $abc "
class MExp_String : public MExp
{
public:
    std::vector<MExp_StringElem> elements;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_String(std::vector<MExp_StringElem>&& elements, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }

};

#pragma endregion Literal

#pragma region List

// [1, 2, 3]
class MExp_List : public MExp
{
public:
    std::vector<MExp*> elems;
    RType* itemType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_List(std::vector<MExp*>&& elems, RType* itemType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_ListIterator : public MExp
{
public:
    MLoc* listLoc;
    RType* iteratorType;
public:
    MIR_API MExp_ListIterator(MLoc* listLoc, RType* iteratorType);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion List

#pragma region Call Internal

enum class MInternalUnaryOperator
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    ToString_Bool_String,
    ToString_Int_String,
};

enum class MInternalUnaryAssignOperator
{
    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,
};

enum class MInternalBinaryOperator
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

class MExp_CallInternalUnaryOperator : public MExp
{
public:
    MInternalUnaryOperator op;
    MExp* operand;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_CallInternalUnaryOperator(MInternalUnaryOperator op, MExp* operand, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_CallInternalUnaryAssignOperator : public MExp
{
public:
    MInternalUnaryAssignOperator op;
    MLoc* operand;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_CallInternalUnaryAssignOperator(MInternalUnaryAssignOperator op, MLoc* operand, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_CallInternalBinaryOperator : public MExp
{
public:
    MInternalBinaryOperator op;
    MExp* operand0;
    MExp* operand1;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_CallInternalBinaryOperator(MInternalBinaryOperator op, MExp* operand0, MExp* operand1, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Call Internal

#pragma region Global

// F();
class MExp_CallGlobalFunc : public MExp
{
public:
    RGlobalFuncDecl* rFuncDecl;
    RTypeArguments* rTypeArgs;
    std::vector<MArgument> args;

public:
    MIR_API MExp_CallGlobalFunc(RGlobalFuncDecl* rFuncDecl, RTypeArguments* rTypeArgs, const std::vector<MArgument>& args);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }

};
#pragma endregion Global

#pragma region Class

// new C(2, 3, 4);
class MExp_NewClass : public MExp
{
public:
    RClassCtorDecl* ctorDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NewClass(RClassCtorDecl* ctorDecl, RTypeArguments* typeArgs, const std::vector<MArgument>& args, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// c.F();
class MExp_CallClassFunc : public MExp
{
public:
    RClassFuncDecl* decl;
    RTypeArguments* typeArgs;
    MLoc* instance;
    std::vector<MArgument> args;
public:
    MIR_API MExp_CallClassFunc(RClassFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// ClassStaticCast
class MExp_CastClass : public MExp
{
public:
    MExp* src;
    RType* classType;
public:
    MIR_API MExp_CastClass(MExp* src, RType* classType);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Class

#pragma region Struct

// S(2, 3, 4);
class MExp_NewStruct : public MExp
{
public:
    RStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, std::vector<MArgument>&& args, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// s.F();
class MExp_CallStructFunc : public MExp
{
public:
    RStructFuncDecl* decl;
    RTypeArguments* typeArgs;
    MLoc* instance;
    std::vector<MArgument> args;
public:
    MIR_API MExp_CallStructFunc(RStructFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Struct

#pragma region Enum

// enum construction, E.First or E.Second(2, 3)
class MExp_NewEnumElem : public MExp
{
public:
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, std::vector<MArgument>&& args, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// 컨테이너를 enumElem -> enum으로
class MExp_CastEnumElemToEnum : public MExp
{
public:
    MExp* src;
    RType* enumType;
public:
    MIR_API MExp_CastEnumElemToEnum(MExp* src, RType* enumType);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Enum

#pragma region Nullable

class MExp_NullableValueNullLiteral : public MExp
{
public:
    RType* innerType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NullableValueNullLiteral(RType* innerType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_NullableRefNullLiteral : public MExp
{
public:
    RType* innerType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NullableRefNullLiteral(RType* innerType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_NewNullable : public MExp
{
public:
    MExp* innerExp;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_NewNullable(MExp* innerExp, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Nullable

#pragma region Lambda

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
class MExp_Lambda : public MExp
{
public:
    NLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_Lambda(NLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, const std::vector<MArgument>& args, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

// f(2, 3)
// Callable은 (() => {}) ()때문에 Loc이어야 한다
class MExp_CallLambda : public MExp
{
public:
    // TODO: RType_Lambda에 있는 정보들, callable->GetType()하면 얻을수 있는 것들이다. 삭제해야 하지 않을까
    RLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;

    MLoc* callable;
    std::vector<MArgument> args;

public:
    MIR_API MExp_CallLambda(RLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, MLoc* callable, const std::vector<MArgument>& args);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Lambda

#pragma region Inline

class MExp_InlineBlock : public MExp
{
public:
    std::vector<MStmt*> stmts;
    RType* returnType;
public:
    MIR_API MExp_InlineBlock(const std::vector<MStmt*>& stmts, RType* returnType);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion Inline

#pragma region TypeTest

class MExp_ClassIsClass : public MExp
{
public:
    MExp* exp;
    RType* classType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_ClassIsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_ClassAsClass: public MExp
{
public:
    MExp* exp;
    RType* classType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_ClassAsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }

};

class MExp_ClassIsInterface : public MExp
{
public:
    MExp* exp;
    RType* interfaceType; // func도 interface type이다
    RFactoryPtr rFactory;

public:
    MIR_API MExp_ClassIsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_ClassAsInterface : public MExp
{
public:
    MExp* exp;
    RType* interfaceType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_ClassAsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_InterfaceIsClass : public MExp
{
public:
    MExp* exp;
    RType* classType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_InterfaceIsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_InterfaceAsClass : public MExp
{
public:
    MExp* exp;
    RType* classType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_InterfaceAsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_InterfaceIsInterface : public MExp
{
public:
    MExp* exp;
    RType* interfaceType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_InterfaceIsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_InterfaceAsInterface : public MExp
{
public:
    MExp* exp;
    RType* interfaceType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_InterfaceAsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_EnumIsEnumElem : public MExp
{
public:
    MExp* exp;
    RType_EnumElem* enumElemType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_EnumIsEnumElem(MExp* exp, RType_EnumElem* enumElemType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

class MExp_EnumAsEnumElem : public MExp
{
public:
    MExp* exp;
    RType_EnumElem* enumElemType;
    RFactoryPtr rFactory;

public:
    MIR_API MExp_EnumAsEnumElem(MExp* exp, RType_EnumElem* enumElemType, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MExpVisitor& visitor) override { visitor.Visit(this); }
};

#pragma endregion TypeTest

template<class TFrom, class TVisitor>
concept MExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<MExp_BitwiseCopy*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_BitwiseAssign*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Stmt*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_StaticBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ClassMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_StructIndirectMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_StructMemberBoxRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_PtrRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_BoolLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_IntLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_String*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_List*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ListIterator*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallInternalUnaryOperator*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallInternalUnaryAssignOperator*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallInternalBinaryOperator*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallGlobalFunc*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallClassFunc*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CastClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewStruct*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallStructFunc*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CastEnumElemToEnum*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewNullable*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NullableValueNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NullableRefNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallLambda*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CastBoxedLambdaToFunc*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InlineBlock*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ClassIsClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ClassAsClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ClassIsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_ClassAsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InterfaceIsClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InterfaceAsClass*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InterfaceIsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InterfaceAsInterface*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_EnumIsEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_EnumAsEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires MExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MExp* mExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(MExp_BitwiseCopy* mExp) override { call(mExp); }
            void Visit(MExp_BitwiseAssign* mExp) override { call(mExp); }
            void Visit(MExp_Stmt* mExp) override { call(mExp); }
            void Visit(MExp_Shared* mExp) override { call(mExp); }
            void Visit(MExp_StaticBoxRef* mExp) override { call(mExp); }
            void Visit(MExp_ClassMemberBoxRef* mExp) override { call(mExp); }
            void Visit(MExp_StructIndirectMemberBoxRef* mExp) override { call(mExp); }
            void Visit(MExp_StructMemberBoxRef* mExp) override { call(mExp); }
            void Visit(MExp_PtrRef* mExp) override { call(mExp); }
            void Visit(MExp_BoolLiteral* mExp) override { call(mExp); }
            void Visit(MExp_IntLiteral* mExp) override { call(mExp); }
            void Visit(MExp_String* mExp) override { call(mExp); }
            void Visit(MExp_List* mExp) override { call(mExp); }
            void Visit(MExp_ListIterator* mExp) override { call(mExp); }
            void Visit(MExp_CallInternalUnaryOperator* mExp) override { call(mExp); }
            void Visit(MExp_CallInternalUnaryAssignOperator* mExp) override { call(mExp); }
            void Visit(MExp_CallInternalBinaryOperator* mExp) override { call(mExp); }
            void Visit(MExp_CallGlobalFunc* mExp) override { call(mExp); }
            void Visit(MExp_NewClass* mExp) override { call(mExp); }
            void Visit(MExp_CallClassFunc* mExp) override { call(mExp); }
            void Visit(MExp_CastClass* mExp) override { call(mExp); }
            void Visit(MExp_NewStruct* mExp) override { call(mExp); }
            void Visit(MExp_CallStructFunc* mExp) override { call(mExp); }
            void Visit(MExp_NewEnumElem* mExp) override { call(mExp); }
            void Visit(MExp_CastEnumElemToEnum* mExp) override { call(mExp); }
            void Visit(MExp_NewNullable* mExp) override { call(mExp); }
            void Visit(MExp_NullableValueNullLiteral* mExp) override { call(mExp); }
            void Visit(MExp_NullableRefNullLiteral* mExp) override { call(mExp); }
            void Visit(MExp_Lambda* mExp) override { call(mExp); }
            void Visit(MExp_CallLambda* mExp) override { call(mExp); }
            void Visit(MExp_CastBoxedLambdaToFunc* mExp) override { call(mExp); }
            void Visit(MExp_InlineBlock* mExp) override { call(mExp); }
            void Visit(MExp_ClassIsClass* mExp) override { call(mExp); }
            void Visit(MExp_ClassAsClass* mExp) override { call(mExp); }
            void Visit(MExp_ClassIsInterface* mExp) override { call(mExp); }
            void Visit(MExp_ClassAsInterface* mExp) override { call(mExp); }
            void Visit(MExp_InterfaceIsClass* mExp) override { call(mExp); }
            void Visit(MExp_InterfaceAsClass* mExp) override { call(mExp); }
            void Visit(MExp_InterfaceIsInterface* mExp) override { call(mExp); }
            void Visit(MExp_InterfaceAsInterface* mExp) override { call(mExp); }
            void Visit(MExp_EnumIsEnumElem* mExp) override { call(mExp); }
            void Visit(MExp_EnumAsEnumElem* mExp) override { call(mExp); }
        };

        Bridge bridge{caller};
        mExp->Accept(bridge);
    }
    else
    {
        struct Bridge : MExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(MExp_BitwiseCopy* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_BitwiseAssign* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Stmt* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Shared* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_StaticBoxRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ClassMemberBoxRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_StructIndirectMemberBoxRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_StructMemberBoxRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_PtrRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_BoolLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_IntLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_String* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_List* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ListIterator* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallInternalUnaryOperator* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallInternalUnaryAssignOperator* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallInternalBinaryOperator* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallGlobalFunc* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallClassFunc* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CastClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewStruct* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallStructFunc* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewEnumElem* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CastEnumElemToEnum* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewNullable* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NullableValueNullLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NullableRefNullLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Lambda* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallLambda* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CastBoxedLambdaToFunc* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InlineBlock* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ClassIsClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ClassAsClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ClassIsInterface* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_ClassAsInterface* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InterfaceIsClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InterfaceAsClass* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InterfaceIsInterface* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InterfaceAsInterface* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_EnumIsEnumElem* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_EnumAsEnumElem* mExp) override { result.emplace(call(mExp)); }
        };

        Bridge bridge{caller};
        mExp->Accept(bridge);
        return *bridge.result;
    }
}

}