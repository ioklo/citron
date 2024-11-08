#include "NModule.h"

#include <cassert>

#include <Infra/Exceptions.h>
#include <Infra/Ptr.h>

#include "RTypeArguments.h"
#include "DeclWithOuterTypeArgs.h"
#include "RGlobalFuncDecl.h"
#include "RTypeFactory.h"
#include "RNamespaceDeclGroup.h"

using namespace std;

namespace Citron {

NModule::NModule(std::string&& name, std::shared_ptr<NNamespaceDecl>&& rootNamespace)
    : name(std::move(name)), rootNamespace(std::move(rootNamespace))
{
}

}