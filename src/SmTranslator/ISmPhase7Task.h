#pragma once
#include <expected>
#include <memory>

namespace Citron {

struct MFuncBody;
using DiagPtr = std::shared_ptr<struct Diag>;
class SmPhase7Context;

class ISmPhase7Task
{
public:
    virtual ~ISmPhase7Task() = default;
    virtual std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) = 0;
};

} // namespace Citron

