#pragma once

namespace Citron {
namespace SyntaxIR0Translator {

class BuildTypeSymbolContext;
class ResolveTypeHierarchyContext;
class BuildTypeDependentSymbolContext;
class SynthesizeImplicitSymbolContext;
class TranslateBodyContext;

class IBuildTypeSymbolTask
{
public:
    ~IBuildTypeSymbolTask() = default;
    virtual void BuildTypeSymbol(BuildTypeSymbolContext& context) = 0;
};

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
    virtual void TranslateBody(TranslateBodyContext& context) = 0;
};


} // namespace SyntaxIR0Translator
} // namespace Citron
