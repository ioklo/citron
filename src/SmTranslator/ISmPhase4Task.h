#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class SmPhase4Context;

// 위의 TypeHierarchy가 있어야 TypeExp를 제대로 resolve할 수 있다
// TypeExp를 resolve하고, 타입을 제외한 나머지 symbol을 추가하는 단계
class ISmPhase4Task
{
public:
    virtual ~ISmPhase4Task() = default;
    virtual std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) = 0;
};

} // namespace Citron