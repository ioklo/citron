#pragma once

#include <string>
#include <vector>
#include <optional>

#include "MCreate.h"
#include "MRead.h"
#include "MArgument.h"
#include "MCallable.h"
#include "MCatch.h"

namespace Citron {

class RType_Func;
class RClassCtorDecl;
class RStructCtorDecl;
class REnumElemDecl;
class NLambdaDecl;

struct MInitExpVisitor;
struct MStmt;
struct MSharedExp;

struct MInitExp
{
    virtual void Accept(MInitExpVisitor& visitor) = 0;
};

struct MInitExp_Shared : MInitExp 
{   
    MCreate create; // BC, NBC 모두 생성가능
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_SharedRef : MInitExp
{
    MSharedExp* sharedExp;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_Stmt : MInitExp
{
    std::vector<MStmt*> stmts;
    MInitExp* finalExp; // BC가 올 일은 없으므로, NBC전용 MInitExp
                        // context dependent, READ인지, CREATE인지 알수 없다

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_StringElem_Text
{
    std::string text;
};

struct MInitExp_StringElem_Loc
{
    MRead_Location loc;
};

using MInitExp_StringElem = std::variant<MInitExp_StringElem_Text, MInitExp_StringElem_Loc>;

// "dskfjslkf $abc "
struct MInitExp_String : MInitExp
{
    std::vector<MInitExp_StringElem> elements;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// [1, 2, 3]
struct MInitExp_List : MInitExp
{
    std::vector<MCreate> elems;
    RType* itemType;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

enum class MInitExp_CallIntrinsicKind
{
    ToString_Bool_String,
    ToString_Int_String,
    Add_String_String_String,
};

struct MInitExp_CallIntrinsic : MInitExp 
{
    MInitExp_CallIntrinsicKind kind;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_NewClass : MInitExp
{
    RClassCtorDecl* ctorDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_StructCtorKind_Copy { MRead_Location src; };
struct MInitExp_StructCtorKind_Move { MMoveSource src; };
struct MInitExp_StructCtorKind_General { RStructCtorDecl* decl; RTypeArguments* typeArgs; std::vector<MArgument> args; };

using MInitExp_StructCtorKind = std::variant<MInitExp_StructCtorKind_Copy, MInitExp_StructCtorKind_Move, MInitExp_StructCtorKind_General>;

struct MInitExp_StructCtor : MInitExp
{   
    MInitExp_StructCtorKind kind;

    MIR_API void Accept(MInitExpVisitor& visitor) override;    
};

// 리턴타입이 NBC일경우
struct MInitExp_Call : MInitExp
{
    MCallable callable;
    std::vector<MArgument> args;
    std::optional<MCatch> o_catch; // try F() catch_* { }이 붙었을 경우
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// Enum이 NBC일 경우
struct MInitExp_NewEnumElem : MInitExp
{
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// NBC value를 nullable(not inplace)로 만들 경우
struct MInitExp_Nullable : MInitExp
{
    MCreate_Init initExp;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// S? s = null;
struct MInitExp_NullableNullLiteral : MInitExp
{
    RType* innerType;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// C? c = null;
struct MInitExp_NullableInplaceNullLiteral : MInitExp
{
    RType* innerType;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

enum class MInitExp_CastKind
{
    Class_Class,
    EnumElemToEnum, 
    SharedLambda_Func
};

// static cast
struct MInitExp_Cast : MInitExp
{
    MInitExp_CastKind kind;
    MRead src;
    RType* targetType;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_Lambda : MInitExp
{
    NLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_InlineBlock : MInitExp
{
    std::vector<MStmt*> stmts;
    RType* returnType;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

enum class MInitExp_AsKind
{
    Class_Class, 
    Class_Interface, 
    Interface_Interface, 
    Enum_EnumElem
};

struct MInitExp_As : MInitExp
{
    MInitExp_AsKind kind;
    MRead operand;
    RType* type;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

MIR_API RType* GetType(MInitExp* initExp, RFactory* rFactory);

} // namespace Citron

#include "MInitExpVisitor.g.h"