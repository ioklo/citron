#pragma once

#include <functional>
#include <memory>
#include <string>

namespace Citron {

class NNamespaceDecl;
class IR0Factory;

namespace SyntaxIR0Translator {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
    IR0Factory& factory;

public:
    SkeletonPhaseContext(IR0Factory& factory);

    NNamespaceDecl* MakeChildNamespace(NNamespaceDecl* decl, const std::string& name);
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);
};

} // SyntaxIR0Translator
} // Citron

