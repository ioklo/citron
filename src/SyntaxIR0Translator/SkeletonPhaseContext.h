#pragma once

#include <functional>
#include <memory>
#include <string>

#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NNamespaceDecl;
class RFactory;
using RFactoryPtr = std::shared_ptr<RFactory>;
class NFactory;
using NFactoryPtr = std::shared_ptr<NFactory>;

namespace SyntaxIR0Translator {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

public:
    SkeletonPhaseContext(const RFactoryPtr& rFactory, const NFactoryPtr& nFactory);

    NNamespaceDecl* MakeChildNamespace(NNamespaceDecl* decl, const std::string& name);
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }
};

} // SyntaxIR0Translator
} // Citron

