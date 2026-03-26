#pragma once
#include "MIRConfig.h"
#include <vector>
#include <variant>
#include "MMoveSource.h"

namespace Citron {

class RType;
class RFactory;

struct MExp;
struct MInitExp;

// 초기화 계획 중 Bitwise Copyable일 경우
struct MCreate_BC { MExp* exp; };

// 초기화 계획 중 Non-bitwise Copyable일 경우
struct MCreate_NBC { MInitExp* initExp; };

// 초기화 계획
using MCreate = std::variant<MCreate_BC, MCreate_NBC>;

MIR_API RType* GetType(MCreate& create, RFactory* rFactory);


//visit([](auto& create) {
//    using T = remove_cvref_t<decltype(create)>;
//    if constexpr (same_as<T, MCreate_BC>)
//    else if constexpr (same_as<T, MCreate_NBC>)
//    else static_assert(false);
//}, create)

} // Citron