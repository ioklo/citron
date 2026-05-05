#include "RNames.h"
#include <format>

using namespace std;

namespace Citron {

namespace RNames {

RName Enumerator;
RName GetEnumerator = RName_Normal("GetEnumerator");
RName Next = RName_Normal("Next");
RName RawItem;
RName _this = RName_Reserved("this"); // "this"
RName _return = RName_Reserved("return");

} // RNames

string RNameToString(const RName& name)
{
    return visit([](auto& name) {
        using T = remove_cvref_t<decltype(name)>;
        if constexpr (same_as<T, RName_Normal>) return name.text;
        else if constexpr (same_as<T, RName_Reserved>) return format("${}", name.text);
        else if constexpr (same_as<T, RName_Lambda>) return format("$$lambdaVar{}>", name.index);
        else if constexpr (same_as<T, RName_CtorParam>) return format("$$ctor_{}", name.paramText);
        else static_assert(false);
    }, name);
}


} // Citron