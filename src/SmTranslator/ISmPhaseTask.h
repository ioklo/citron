#pragma once

namespace Citron {

// SmPhaseManager를 사용하는 Task들의 공통 인터페이스
class ISmPhaseTask
{
public:
    virtual ~ISmPhaseTask() = default;
};

} // namespace Citron