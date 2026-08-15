#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class RType;

class SmType;
class SmFactory;

SmType* TranslateRTypeToSmType(RType* rType, SmFactory* factory);

} // namespace Citron
