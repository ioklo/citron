#pragma once
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class SmPhase6Context;

// struct / class 등에 자동으로 생성되는 implicit symbol을 추가하는 단계 (memberwise constructor 등)
class ISmPhase6Task
{
public:
    virtual ~ISmPhase6Task() = default;
    virtual void RunPhase6(SmPhase6Context& context) = 0;
};

} // namespace Citron


