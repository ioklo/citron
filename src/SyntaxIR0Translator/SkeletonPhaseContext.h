#pragma once

#include <functional>

namespace Citron {

class NNamespaceDecl;

namespace SyntaxIR0Translator {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
    RTypeFactory& factory;

public:
    SkeletonPhaseContext(RTypeFactory& factory);

    std::shared_ptr<NNamespaceDecl> MakeChildNamespace(const std::shared_ptr<NNamespaceDecl>& decl, const std::string& name);
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);
};

} // SyntaxIR0Translator

} // Citron
