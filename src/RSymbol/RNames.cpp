#include "RNames.h"
#include <format>

using namespace std;

namespace Citron {

namespace RNames {

RName Enumerator = RName_Normal{"Enumerator"};
RName GetEnumerator = RName_Normal{"GetEnumerator"};
RName Next = RName_Normal{"Next"};
RName RawItem = RName_Normal{"RawItem"};
RName _this = RName_Reserved{RName_ReservedName::This}; // "this"
RName _return = RName_Reserved{RName_ReservedName::Return};

} // RNames

string RName::ToString()
{
    return visit([](auto& name) -> string {
        using T = remove_cvref_t<decltype(name)>;
        if constexpr (same_as<T, RName_None>) return "(None)";
        else if constexpr (same_as<T, RName_Normal>) return name.text;
        else if constexpr (same_as<T, RName_Reserved>) return format("${}", static_cast<int>(name.name));
        else if constexpr (same_as<T, RName_Lambda>) return format("$$lambdaVar{}>", name.index);
        else if constexpr (same_as<T, RName_CtorParam>) return format("$$ctor_{}", name.paramText);
        else static_assert(false);
    }, v);
}

std::string RName_ReservedNameToString(InRef<RName_ReservedName> name)
{
    switch (*name)
    {
    case RName_ReservedName::Enumerator: return "(Enumerator)";
    case RName_ReservedName::GetEnumerator: return "(GetEnumerator)";
    case RName_ReservedName::Next: return "(Next)";
    case RName_ReservedName::RawItem: return "(RawItem)";
    case RName_ReservedName::This: return "(this)";
    case RName_ReservedName::Return: return "(return)";
    case RName_ReservedName::Ctor: return "(ctor)";
    case RName_ReservedName::Dtor: return "(dtor)";
    default:
        throw std::runtime_error("Unknown RName_ReservedName");
    }
}

} // Citron