#include "RNames.h"
#include <format>

using namespace std;

namespace Citron {

namespace RNames {

RName Enumerator = RName_Normal{"Enumerator"};
RName GetEnumerator = RName_Normal{"GetEnumerator"};
RName Next = RName_Normal{"Next"};
RName RawItem = RName_Normal{"RawItem"};
RName _this = RName_Normal{"this"}; // "this"
RName _return = RName_Normal{"return"};

} // RNames

string RName::ToString()
{
    return visit([](auto& name) -> string {
        using T = remove_cvref_t<decltype(name)>;
        if constexpr (same_as<T, RName_Normal>) return name.text;
        else static_assert(false);
    }, v);
}

} // Citron