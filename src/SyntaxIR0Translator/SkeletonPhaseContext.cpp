#include "SkeletonPhaseContext.h"

#include <memory>
#include <string>

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

SkeletonPhaseContext::SkeletonPhaseContext(IR0Factory& factory)
    : factory(factory)
{
    throw NotImplementedException();
}

NNamespaceDecl* SkeletonPhaseContext::MakeChildNamespace(NNamespaceDecl* decl, const string& name)
{

    throw NotImplementedException();
}

void SkeletonPhaseContext::AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f)
{
    throw NotImplementedException();
}

} // Citron::SyntaxIR0Translator