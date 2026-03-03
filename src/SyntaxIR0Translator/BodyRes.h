#pragma once

#include "RSymbol/RDeclRes.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RType;
// Body-Space Resolved Result

using BodyRes = std::variant<
    struct BodyRes_RDeclRes,
    struct BodyRes_LocalVar,
    struct BodyRes_LocalRef,
    struct BodyRes_NeedCapture, // 람다에서 캡쳐가 필요할때
    struct BodyRes_ThisVar
>;

struct BodyRes_RDeclRes { RDeclRes declRes; };
struct BodyRes_LocalVar { RType* type; RName name; };
struct BodyRes_LocalRef { RType* type; RName name; };
struct BodyRes_NeedCapture { RName name; std::unique_ptr<BodyRes> member; };
struct BodyRes_ThisVar { RType* type; };

} // namespace Citron
