#pragma once

#include <functional>
#include <memory>
#include <string>

#include "IR0/RFactory.h"

namespace Citron {

class NNamespaceDecl;
class RFactory;

namespace SyntaxIR0Translator {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
    RFactory& rFactory;

public:
    SkeletonPhaseContext(RFactory& rFactory);

    NNamespaceDecl* MakeChildNamespace(NNamespaceDecl* decl, const std::string& name);
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    constexpr TNDecl* MakeNDecl(TArgs&&... args)
    {
        return rFactory.MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }
};

} // SyntaxIR0Translator
} // Citron

