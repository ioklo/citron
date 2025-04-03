export module Citron.SyntaxIR0Translator:SkeletonPhaseContext;

import <functional>;
import <memory>;
import <string>;

import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class MemberDeclPhaseContext;

export class SkeletonPhaseContext
{
    RTypeFactory& factory;

public:
    SkeletonPhaseContext(RTypeFactory& factory);

    std::shared_ptr<NNamespaceDecl> MakeChildNamespace(const std::shared_ptr<NNamespaceDecl>& decl, const std::string& name);
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);
};

} // Citron::SyntaxIR0Translator

