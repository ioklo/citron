#pragma once

#include <string>
#include <vector>
#include <optional>
#include <variant>

#include "MCreate.h"
#include "MRead.h"
#include "MArgument.h"
#include "MCallable.h"
#include "MCatch.h"

namespace Citron {

class RType_Func;
class RType_Struct;
class RClassCtorDecl;
class RStructCtorDecl;
class REnumElemDecl;
class RLambdaDecl;

struct MInitExpVisitor;
struct MStmt;
struct MStmt_Scope;
struct MSharedExp;

struct MInitExp
{
    virtual ~MInitExp() { }
    virtual void Accept(MInitExpVisitor& visitor) = 0;
};

struct MInitExp_Shared : MInitExp 
{   
    MCreate create; // BC, NBC 모두 생성가능
    MInitExp_Shared(MCreate&& create)
        : create{std::move(create)}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_SharedRef : MInitExp
{
    MSharedExp* sharedExp;

    MInitExp_SharedRef(MSharedExp* sharedExp)
        : sharedExp{sharedExp}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_Stmt : MInitExp
{
    std::vector<MStmt*> stmts;
    MInitExp* finalExp; // BC가 올 일은 없으므로, NBC전용 MInitExp
                        // context dependent, READ인지, CREATE인지 알수 없다

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_StringElem_Text { std::string text; };
struct MInitExp_StringElem_InitExp { MInitExp* initExp; };
struct MInitExp_StringElem_Loc { MLoc* loc; };

using MInitExp_StringElem = std::variant<MInitExp_StringElem_Text, MInitExp_StringElem_InitExp, MInitExp_StringElem_Loc>;

// "dskfjslkf $abc "
struct MInitExp_String : MInitExp
{
    std::vector<MInitExp_StringElem> elements;
    MInitExp_String(std::vector<MInitExp_StringElem>&& elements)
        : elements{std::move(elements)}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// [1, 2, 3]
struct MInitExp_List : MInitExp
{
    std::vector<MCreate> elems;
    RType* itemType;

    MInitExp_List(std::vector<MCreate>&& elems, RType* itemType)
        : elems{std::move(elems)}, itemType{itemType}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

enum class MInitExp_CallIntrinsicKind
{
    ToString_String_Bool,
    ToString_String_Int,
    Add_String_StringInRef_StringInRef,
    Max,
};

struct MInitExp_CallIntrinsic : MInitExp 
{
    MInitExp_CallIntrinsicKind kind;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MInitExp_CallIntrinsic(MInitExp_CallIntrinsicKind kind, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : kind{kind}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_NewClass : MInitExp
{
    RClassCtorDecl* ctorDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_StructCtorKind_Copy { RType_Struct* structType; MRead_Loc src; };
struct MInitExp_StructCtorKind_Move { RType_Struct* structType; MMoveSource src; };
struct MInitExp_StructCtorKind_General { RStructCtorDecl* decl; RTypeArguments* typeArgs; std::vector<MArgument> args; };

using MInitExp_StructCtorKind = std::variant<MInitExp_StructCtorKind_Copy, MInitExp_StructCtorKind_Move, MInitExp_StructCtorKind_General>;

struct MInitExp_StructCtor : MInitExp
{
    MInitExp_StructCtorKind kind;

    MInitExp_StructCtor(MInitExp_StructCtorKind kind)
        : kind{kind}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;    
};

// 리턴타입이 NBC일경우
struct MInitExp_Call : MInitExp
{
    MCallable callable;
    std::vector<MArgument> args;
    std::optional<MCatch> o_catch; // try F() catch_* { }이 붙었을 경우

    MInitExp_Call(MCallable&& callable, std::vector<MArgument>&& args, std::optional<MCatch>&& o_catch)
        : callable{std::move(callable)}, args{std::move(args)}, o_catch{std::move(o_catch)}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// Enum이 NBC일 경우
struct MInitExp_NewEnumElem : MInitExp
{
    REnumElemDecl* enumElemDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MInitExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : enumElemDecl{enumElemDecl}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// NBC value를 nullable(not inplace)로 만들 경우
struct MInitExp_Nullable : MInitExp
{
    MCreate_NBC inner;
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// S? s = null;
struct MInitExp_NullableNullLiteral : MInitExp
{
    RType* innerType;

    MInitExp_NullableNullLiteral(RType* innerType)
        : innerType{innerType}
    { }
    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

// C? c = null;
struct MInitExp_NullableInplaceNullLiteral : MInitExp
{
    RType* innerType;

    MInitExp_NullableInplaceNullLiteral(RType* innerType)
        : innerType{innerType}
    { }
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
    RLambdaDecl* lambdaDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

struct MInitExp_InlineBlock : MInitExp
{
    MStmt_Scope* scope;
    RType* returnType;

    MInitExp_InlineBlock(MStmt_Scope* scope, RType* returnType)
        : scope{scope}, returnType{returnType}
    {
    }

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

enum class MInitExp_AsKind
{
    Class_Class, 
    Class_Interface, 
    Interface_Class,
    Interface_Interface
};

struct MInitExp_As : MInitExp
{
    MInitExp_AsKind kind;
    MRead target;
    RType* type;

    MInitExp_As(MInitExp_AsKind kind, MRead&& target, RType* type)
        : kind{kind}, target{std::move(target)}, type{type}
    { }

    MIR_API void Accept(MInitExpVisitor& visitor) override;
};

MIR_API RType* GetType(MInitExp_StructCtorKind& ctorKind, RFactory* rFactory);
MIR_API RType* GetType(MInitExp* initExp, RFactory* rFactory);

} // namespace Citron

#include "MInitExpVisitor.g.h"