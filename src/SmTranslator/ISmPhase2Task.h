#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SmPhase2Context;

// base struct / interface 관계를 resolve 하고 상속 트리를 확정하는 단계
class ISmPhase2Task
{
public:
    virtual ~ISmPhase2Task() = default;
    virtual std::expected<void, DiagPtr> RunPhase2(SmPhase2Context& context) = 0;
};

} // namespace Citron