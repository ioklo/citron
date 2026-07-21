#include "RIdentifier.h"
#include <sstream>
#include <format>
#include "Infra/Exceptions.h"
#include "RNames.h"
#include "RTypes.h"
#include "RTypeParam.h"
#include "RClassDecl.h"
#include "RStructDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RInterfaceDecl.h"
#include "RGlobalIdentifier.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

// RIdentifier가 될 수 있는건 Decl

// $ . , < > 
string EncodeText(InRef<string> text)
{
    ostringstream oss;
    for (char c : *text)
    {
        if (c == '$')
            oss << "$$";
        else if (c == '.')
            oss << "$.";
        else if (c == ',')
            oss << "$,";
        else if (c == '<')
            oss << "$<";
        else if (c == '>')
            oss << "$>";
        else if (c == '(')
            oss << "$(";
        else if (c == ')')
            oss << "$)";
        else
            oss << c;
    }
}

string EncodeRName_Reserved(InRef<RName_Reserved> name)
{
    switch (name->name)
    {
    case RName_ReservedName::Enumerator:
        return "$RE";
    case RName_ReservedName::GetEnumerator:
        return "$RG";
    case RName_ReservedName::Next:
        return "$RN";
    case RName_ReservedName::RawItem:
        return "$RR";
    case RName_ReservedName::This:
        return "$RT";
    case RName_ReservedName::Return:
        return "$Rr";
    case RName_ReservedName::Ctor:
        return "$RC";
    case RName_ReservedName::Dtor:
        return "$RD";
    }
}

string EncodeRName_Lambda(InRef<RName_Lambda> name)
{
    return "$L" + to_string(name->index);
}

string EncodeRName_CtorParam(InRef<RName_CtorParam> name)
{
    return format("$C({},{})", name->index, EncodeText(name->paramText));
}

string EncodeRName_ImplTrait(InRef<RName_ImplTrait> name)
{
    return format("$I({},{})", name->declId.text, name->traitId.text);
}

string EncodeRName(InRef<RName> name)
{
    name->Visit([](auto& name) -> string {

        using T = remove_cvref_t<decltype(name)>;
        if constexpr (same_as<T, RName_None>)
            return "$N";
        else if constexpr (same_as<T, RName_Normal>)
            return EncodeText(name.text);
        else if constexpr (same_as<T, RName_Reserved>)
            return EncodeRName_Reserved(name);
        else if constexpr (same_as<T, RName_Lambda>)
            return EncodeRName_Lambda(name);
        else if constexpr (same_as<T, RName_CtorParam>)
            return EncodeRName_CtorParam(name);
        else if constexpr (same_as<T, RName_ImplTrait>)
            return EncodeRName_ImplTrait(name);
        else
            static_assert(false);
    });
}

string EncodeRType(RType* type);

struct RTypeEncoder
{
    using ResultType = string;

    string EncodeTypeArgs(RTypeArguments* typeArgs)
    {
        size_t count = typeArgs->GetCount();
        if (count == 0) return "";

        ostringstream oss;
        oss << "<";
        bool bFirst = true;
        for (size_t i = 0; i < count; i++)
        {
            if (bFirst) bFirst = false;
            else oss << ",";
            oss << EncodeRType(typeArgs->Get(i));
        }
        oss << ">";
        return oss.str();
    }

    string EncodeDeclAndTypeArgs(RDecl* decl, RTypeArguments* typeArgs)
    {
        auto globalId = GetRGlobalIdentifier(decl);
        return format("{}{}", globalId.text, EncodeTypeArgs(typeArgs));
    }

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
    }

    string Visit(RType_Tuple* rType) 
    {
        ostringstream oss{"$t<"};

        bool bFirst = true;
        for (auto& var : rType->vars)
        {
            if (bFirst) bFirst = false;
            else oss << ",";

            oss << EncodeRType(var.declType);
        }
        oss << ">";

        return oss.str();
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
}

string EncodeRFuncParam(RFuncParameter& funcParam)
{
    return format("{}{}", EncodeRFuncParamKind(funcParam.kind), EncodeRType(funcParam.type));
}

string EncodeFuncName(InRef<RName> name, span<RFuncParameter> funcParams)
{
    ostringstream oss;
    
    oss << EncodeRName(name) << "(";
    bool bFirst = true;
    for (auto& funcParam : funcParams)
    {
        if (bFirst) bFirst = false;
        else oss << ",";
        oss << EncodeRFuncParam(funcParam);
    }
    oss << ")";
    return oss.str();
}

} // namespace Citron