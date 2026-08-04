#include "RDeclKey.h"
#include <format>
#include "RNames.h"
#include "RTypeArguments.h"
#include "RTypes.h"
#include "RTypeParam.h"
#include "RClassDecl.h"
#include "RStructDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RInterfaceDecl.h"
#include "RTypeIdentifier.h"
#include "RTraitDecl.h"


using namespace std;

namespace Citron {

// RDeclKey가 될 수 있는건 Decl

// $ . , < > 
string EncodeText(string_view text)
{
    string buffer;
    buffer.reserve(text.size());
    for (char c : text)
    {
        if (c == '$')
            buffer += "$$";
        else if (c == '.')
            buffer += "$.";
        else if (c == ',')
            buffer += "$,";
        else if (c == '<')
            buffer += "$<";
        else if (c == '>')
            buffer += "$>";
        else if (c == '(')
            buffer += "$(";
        else if (c == ')')
            buffer += "$)";
        else
            buffer += c;
    }

    return buffer;
}

string EncodeRName(InRef<RName> name)
{
    return name->Visit([](auto& name) -> string {

        using T = remove_cvref_t<decltype(name)>;
        if constexpr (same_as<T, RName_Normal>)
            return EncodeText(name.text);
        else 
            static_assert(false);
    });
}

string EncodeRType(RType* type);

string EncodeTypeArgs(RTypeArguments* typeArgs)
{
    size_t count = typeArgs->GetCount();
    if (count == 0) return "";

    string buffer;
    buffer += "<";
    bool bFirst = true;
    for (size_t i = 0; i < count; i++)
    {
        if (bFirst) bFirst = false;
        else buffer += ",";
        buffer += EncodeRType(typeArgs->Get(i));
    }
    buffer += ">";
    return buffer;
}

string EncodeDeclAndTypeArgs(RDecl* decl, RTypeArguments* typeArgs)
{
    auto identifier = decl->GetIdentifier();
    return format("{}{}", identifier.text, EncodeTypeArgs(typeArgs));
}

template<typename TRDecl>
string EncodeRAppliedDecl(RAppliedDecl<TRDecl> appliedDecl)
{
    return EncodeDeclAndTypeArgs(appliedDecl.decl, appliedDecl.typeArgs);
}

struct RTypeEncoder
{
    using ResultType = string;

    string Visit(RType_Nullable* rType)
    {
        return format("$N{}", EncodeRType(rType->innerType));
    }

    string Visit(RType_NullableInplace* rType)
    {
        return format("$n{}", EncodeRType(rType->innerType));
    }

    string Visit(RType_TypeVar* rType)
    {
        return format("$T{}", rType->typeParam->GetGlobalIndex());
    }

    string Visit(RType_Void* rType)
    {
        return "$V";
    }

    string Visit(RType_Primitive* rType)
    {
        switch (rType->GetPrimitiveKind())
        {
        case RType_PrimitiveKind::Bool:
            return "$pb";

        case RType_PrimitiveKind::Int32:
            return "$pi";
        }
        unreachable();
    }

    string Visit(RType_Tuple* rType)
    {
        string buffer;
        buffer += "$t<";

        bool bFirst = true;
        for (auto& var : rType->vars)
        {
            if (bFirst) bFirst = false;
            else buffer += ",";

            buffer += EncodeRType(var.declType);
        }
        buffer += ">";

        return buffer;
    }

    // 
    string Visit(RType_Func* rType)
    {
        // TODO: [72] 2026-07-21, func<> 타입 구현
        throw NotImplementedException{};
    }

    string Visit(RType_Ptr* rType)
    {
        return format("$P{}", EncodeRType(rType->innerType));
    }

    string Visit(RType_Shared* rType)
    {
        return format("$S{}", EncodeRType(rType->innerType));
    }

    string Visit(RType_Box* rType)
    {
        return format("$B{}", EncodeRType(rType->innerType));
    }

    string Visit(RType_Class* rType)
    {
        return EncodeDeclAndTypeArgs(rType->decl, rType->typeArgs);
    }

    string Visit(RType_Struct* rType)
    {
        return EncodeDeclAndTypeArgs(rType->decl, rType->typeArgs);
    }

    string Visit(RType_Enum* rType)
    {
        return EncodeDeclAndTypeArgs(rType->decl, rType->typeArgs);
    }

    string Visit(RType_EnumElem* rType)
    {
        return EncodeDeclAndTypeArgs(rType->decl, rType->typeArgs);
    }

    string Visit(RType_Interface* rType)
    {
        return EncodeDeclAndTypeArgs(rType->decl, rType->typeArgs);
    }

    string Visit(RType_Lambda* rType)
    {
        // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
        throw NotImplementedException{};
    }

    string Visit(RType_Opaque* rType)
    {
        return format("$O({},{})", 
            EncodeRAppliedDecl(rType->appliedTrait),
            EncodeRAppliedDecl(rType->appliedOwnerFunc));
    }
};

string EncodeRType(RType* type)
{
    return Accept(RTypeEncoder{}, type);
}

string EncodeRFuncParamKind(RFuncParameterKind kind)
{
    switch (kind)
    {
    case RFuncParameterKind::Normal: return "N";
    case RFuncParameterKind::Ref: return "R";
    case RFuncParameterKind::In: return "I";
    case RFuncParameterKind::Move: return "M";
    case RFuncParameterKind::Forward: return "F";
    case RFuncParameterKind::Out: return "O";
    case RFuncParameterKind::Params: return "P";
    case RFuncParameterKind::Init: return "i";
    }
    unreachable();
}

string EncodeRFuncParam(RFuncParameter& funcParam)
{
    return format("{}{}", EncodeRFuncParamKind(funcParam.kind), EncodeRType(funcParam.type));
}

string EncodeFuncName(InRef<RName> name, span<RFuncParameter> funcParams)
{
    string buffer;
    buffer += EncodeRName(name);
    buffer += "(";
    bool bFirst = true;
    for (auto& funcParam : funcParams)
    {
        if (bFirst) bFirst = false;
        else buffer += ",";
        buffer += EncodeRFuncParam(funcParam);
    }
    buffer += ")";
    return buffer;
}

RDeclKey RDeclKey::Normal(InRef<RName> name)
{
    return RDeclKey{EncodeRName(name)};
}

RDeclKey RDeclKey::Func(InRef<RName> name, std::span<RFuncParameter> funcParams)
{
    string buffer;
    buffer += "F(";
    buffer += EncodeRName(name);
    bool bFirst = true;
    for (auto& funcParam : funcParams)
    {
        if (bFirst) bFirst = false;
        else buffer += ",";
        buffer += EncodeRFuncParam(funcParam);
    }
    buffer += ")";
    return RDeclKey{buffer};
}

RDeclKey RDeclKey::Ctor(std::span<RFuncParameter> funcParams)
{
    string buffer;
    buffer += "C(";
    bool bFirst = true;
    for (auto& funcParam : funcParams)
    {
        if (bFirst) bFirst = false;
        else buffer += ",";
        buffer += EncodeRFuncParam(funcParam);
    }
    buffer += ")";
    return RDeclKey{buffer};
}

RDeclKey RDeclKey::Dtor()
{
    return RDeclKey{"D()"};
}

RDeclKey RDeclKey::ImplTrait(RDecl* target, RAppliedDecl<RTraitDecl> appliedTraitDecl)
{
    return RDeclKey{format("I({},{})", target->GetDeclKey().GetValue(), EncodeRAppliedDecl(appliedTraitDecl))};
}

} // namespace Citron