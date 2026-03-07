#pragma once
#include "MIRConfig.h"

#include <variant>
#include <vector>

#include "MCreate.h"
#include "MRead.h"
#include "MCatch.h"
#include "MCallable.h"
#include "MArgument.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RType;
class RType_Class;
class RType_EnumElem;
class RFactory;
class RStructCtorDecl;
class REnumElemDecl;
class RTypeArguments;

struct MLoc;
struct MStmt;
class NLambdaDecl;

struct MExpVisitor;

struct MExp
{
public:
    virtual ~MExp() {}
    virtual void Accept(MExpVisitor& visitor) = 0;
};

// 주어진 Location의 값을 비트단위로 복사한다
struct MExp_Load : MExp
{
    MLoc* loc;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// 주어진 Exp를 값으로 계산해서 Location에 비트단위로 복사하고, 그 값을 나타낸다
struct MExp_Store : MExp
{
    MLoc* dest;
    MExp* src;

    MIR_API void Accept(MExpVisitor& visitor) override;
};

// { stmt1; stmt2; ...; finalExp; }
struct MExp_Stmt : MExp
{
    std::vector<MStmt*> stmts;
    MExp* finalExp;
    
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// var* p = &i; // &i
struct MExp_PtrRef : MExp
{
    MLoc* innerLoc;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// false
struct MExp_BoolLiteral : MExp
{
    bool value;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// 1
struct MExp_IntLiteral : MExp
{
    int value;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

enum class MExp_CallIntrinsicKind
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,

    Multiply_Int_Int_Int,
    Divide_Int_Int_Int,
    Modulo_Int_Int_Int,
    Add_Int_Int_Int,
    
    Subtract_Int_Int_Int,
    LessThan_Int_Int_Bool,
    LessThan_String_String_Bool, // [in]string&으로 받아야 한다
    GreaterThan_Int_Int_Bool,
    GreaterThan_String_String_Bool,
    LessThanOrEqual_Int_Int_Bool,
    LessThanOrEqual_String_String_Bool,
    GreaterThanOrEqual_Int_Int_Bool,
    GreaterThanOrEqual_String_String_Bool,
    Equal_Int_Int_Bool,
    Equal_Bool_Bool_Bool,
    Equal_String_String_Bool,

    GetIterator_List_ListIterator, // 첫번째 타입 파라미터는 ItemType
};

struct MExp_CallIntrinsic : MExp
{
    MExp_CallIntrinsicKind kind;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_Call : MExp
{
    MCallable callable;
    std::vector<MArgument> args;
    std::optional<MCatch> o_catch; // try F() catch_* { }이 붙었을 경우

    MIR_API void Accept(MExpVisitor& visitor) override;
};

// S(2, 3, 4);
struct MExp_NewStruct : MExp
{
    RStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// enum construction, E.First or E.Second(2, 3)
struct MExp_NewEnumElem : MExp
{
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_Nullable : MExp
{
    MExp* innerExp;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_NullableNullLiteral : MExp
{
    RType* innerType;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

enum class MExp_CastKind
{
    EnumElem_Enum
};

// 컨테이너를 enumElem -> enum으로
struct MExp_Cast : MExp
{
    MExp_CastKind kind;
    MRead src;
    RType* targetType;
    
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// int x = 1;
// var l = () => { return x; }; // lambda type
//
// Lambda(lambda_type_0, x); // with captured variable
struct MExp_Lambda : MExp
{
    NLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_InlineBlock : MExp
{
    std::vector<MStmt*> stmts;
    RType* returnType;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

using MPatternLeaf = std::variant<struct MPattern_Alias, struct MPattern_Ignore>;
using MTopLevelPattern = std::variant<struct MPattern_Ignore, struct MPattern_Null, struct MPattern_Some, struct MPattern_Class, struct MPattern_EnumElem>;
using MPattern = std::variant<struct MPattern_Alias, struct MPattern_Ignore, struct MPattern_Null, struct MPattern_Some, struct MPattern_Class, struct MPattern_EnumElem>;

struct MPattern_Alias { RName name; };
struct MPattern_Ignore {};
struct MPattern_Null {};
struct MPattern_Some { MPatternLeaf pattern; }; // some _, some a
struct MPattern_Class { RType_Class* type; MPatternLeaf pattern; }; // C _
struct MPattern_EnumElem { RType_EnumElem* type; std::vector<MPattern> patterns; }; // E(C c, some s, a, _) 또는 E e

// is pattern 
// c is null
// c is some
// c is some s
// c is D
// c is D d
// e is E.Second(x, _)
struct MExp_Is : MExp
{   
    MRead operand; // BC/NBC를 모두 받을 수 있는 방법.
    MTopLevelPattern pattern;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

enum class MExp_AsKind
{
    Enum_EnumElem,
};

struct MExp_As : MExp
{
    MExp_AsKind kind;
    MRead operand;
    RType* type;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_Try : MExp
{
    MExp* exp;
    MCatch _catch; // 단일 catch만 가능하다

    MIR_API void Accept(MExpVisitor& visitor) override;
};

MIR_API RType* GetType(MExp* exp, RFactory* rFactory);

}

#include "MExpVisitor.g.h"