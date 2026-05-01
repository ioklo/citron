#pragma once
#include <memory>
#include <expected>

namespace Citron {
struct MFuncBody;
using DiagPtr = std::shared_ptr<struct Diag>;

class ResolveTypeHierarchyContext;
class BuildTypeDependentSymbolContext;
class SynthesizeImplicitSymbolContext;
class TranslateBodyContext;

// base struct / interface 관계를 resolve 하고 상속 트리를 확정하는 단계
class IResolveTypeHierarchyTask
{
public:
    ~IResolveTypeHierarchyTask() = default;
    virtual void ResolveTypeHierarchy(ResolveTypeHierarchyContext& context) = 0;
};

class IBuildTypeDependentSymbolTask
{
public:
    ~IBuildTypeDependentSymbolTask() = default;
    virtual std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) = 0;
};

class ISynthesizeImplicitSymbolTask
{
public:
    ~ISynthesizeImplicitSymbolTask() = default;
    virtual void SynthesizeImplicitSymbol(SynthesizeImplicitSymbolContext& context) = 0;
};

class ITranslateBodyTask
{
public:
    ~ITranslateBodyTask() = default;
    virtual std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) = 0;
};


} // namespace Citron
