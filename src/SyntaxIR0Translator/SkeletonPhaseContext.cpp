module Citron.SyntaxIR0Translator:SkeletonPhaseContext;

import <memory>;
import <string>;

import Citron.Exceptions;

using namespace std;

namespace Citron::SyntaxIR0Translator {

SkeletonPhaseContext::SkeletonPhaseContext(RTypeFactory& factory)
    : factory(factory)
{
    throw NotImplementedException();
}

shared_ptr<NNamespaceDecl> SkeletonPhaseContext::MakeChildNamespace(const shared_ptr<NNamespaceDecl>& decl, const string& name)
{

    throw NotImplementedException();
}

void SkeletonPhaseContext::AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f)
{
    throw NotImplementedException();
}

} // Citron::SyntaxIR0Translator