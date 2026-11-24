#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct SmPhase5Contexts;

// BuildNonTypeSymbolPhase가 끝난뒤 수행할 작업.
class ISmPhase5Task
{
public:
    virtual ~ISmPhase5Task() = default;
    virtual std::expected<void, DiagPtr> RunPhase5(SmPhase5Contexts& context) = 0;
};

} // namespace Citron
