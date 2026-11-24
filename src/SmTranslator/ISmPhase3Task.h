#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct SmPhase3Contexts;

class ISmPhase3Task
{
public:
    virtual ~ISmPhase3Task() = default;
    virtual std::expected<void, DiagPtr> RunPhase3(SmPhase3Contexts& contexts) = 0;
};

} // namespace Citron