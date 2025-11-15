#pragma once
#include <memory>
#include <expected>

namespace Citron {
struct MFuncBody;
using DiagPtr = std::shared_ptr<struct Diag>;

namespace SyntaxIR0Translator {

class ResolveTypeHierarchyContext;
class BuildTypeDependentSymbolContext;
class SynthesizeImplicitSymbolContext;
class TranslateBodyContext;

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
    virtual void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) = 0;
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


} // namespace SyntaxIR0Translator
} // namespace Citron
