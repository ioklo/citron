#pragma once
#include "MIRConfig.h"

#include <variant>
#include <vector>
#include <optional>

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
struct MStmt_Scope;
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

    MExp_Load(MLoc* loc)
        : loc{loc}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// 주어진 Exp를 값으로 계산해서 Location에 비트단위로 복사하고, 그 값을 나타낸다
struct MExp_Store : MExp
{
    MLoc* dest;
    MRead src;  // BC

    MExp_Store(MLoc* dest, MRead&& src)
        : dest{dest}, src{std::move(src)}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// { stmt1; stmt2; ...; finalExp; }
struct MExp_Stmt : MExp
{
    std::vector<MStmt*> stmts;
    MExp* finalExp; // context dependent, READ인지, CREATE인지 알수 없다

    MExp_Stmt(std::vector<MStmt*>&& stmts, MExp* finalExp)
        : stmts{std::move(stmts)}, finalExp{finalExp}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// var* p = &i; // &i
struct MExp_PtrRef : MExp
{
    MLoc* innerLoc;

    MExp_PtrRef(MLoc* innerLoc)
        : innerLoc{innerLoc}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// false
struct MExp_BoolLiteral : MExp
{
    bool value;

    MExp_BoolLiteral(bool value)
        : value{value}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// 1
struct MExp_IntLiteral : MExp
{
    int value;
    MExp_IntLiteral(int value)
        : value{value}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

enum class MExp_CallIntrinsicKind
{
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,

    PrefixInc_Int_IntRef,
    PrefixDec_Int_IntRef,
    PostfixInc_Int_IntRef,
    PostfixDec_Int_IntRef,

    Multiply_Int_Int_Int,
    Divide_Int_Int_Int,
    Modulo_Int_Int_Int,
    Add_Int_Int_Int,
    
    Subtract_Int_Int_Int,
    LessThan_Bool_Int_Int,
    LessThan_Bool_StringInRef_StringInRef, // [in]string&으로 받아야 한다
    GreaterThan_Bool_Int_Int,
    GreaterThan_Bool_StringInRef_StringInRef,
    LessThanOrEqual_Bool_Int_Int,
    LessThanOrEqual_Bool_StringInRef_StringInRef,
    GreaterThanOrEqual_Bool_Int_Int,
    GreaterThanOrEqual_Bool_StringInRef_StringInRef,
    Equal_Bool_Int_Int,
    Equal_Bool_Bool_Bool,
    Equal_Bool_StringInRef_StringInRef,

    GetIterator_ListPtr_ListIterator, // 첫번째 타입 파라미터는 ItemType
    Max,
};

struct MExp_CallIntrinsic : MExp
{
    MExp_CallIntrinsicKind kind;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MExp_CallIntrinsic(MExp_CallIntrinsicKind kind, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : kind{kind}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_Call : MExp
{
    MCallable callable;
    std::vector<MArgument> args;
    std::optional<MCatch> o_catch; // try F() catch_* { }이 붙었을 경우

    MExp_Call(MCallable&& callable, std::vector<MArgument>&& args, std::optional<MCatch>&& o_catch)
        : callable{std::move(callable)}, args{std::move(args)}, o_catch{std::move(o_catch)}
    { }

    MIR_API void Accept(MExpVisitor& visitor) override;
};

// S(2, 3, 4);
struct MExp_NewStruct : MExp
{
    RStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : ctor{ctor}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

// enum construction, E.First or E.Second(2, 3)
struct MExp_NewEnumElem : MExp
{
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : enumElemDecl{enumElemDecl}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_Nullable : MExp
{
    MCreate_BC innerExp;
    MIR_API void Accept(MExpVisitor& visitor) override;
};

struct MExp_NullableNullLiteral : MExp
{
    RType* innerType;

    MExp_NullableNullLiteral(RType* innerType)
        : innerType{innerType}
    { }
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
    MStmt_Scope* scope;
    RType* returnType;

    MExp_InlineBlock(MStmt_Scope* scope, RType* returnType)
        : scope{scope}, returnType{returnType}
    {
    }

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

MIR_API RType* GetType(MExp* exp, RFactory* rFactory);

}

#include "MExpVisitor.g.h"