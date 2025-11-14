#include "BuildTypeSymbolContext.h"

#include <memory>
#include <string>

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

BuildTypeSymbolContext::BuildTypeSymbolContext(const NFactoryPtr& nFactory)
    : nFactory{nFactory}
{   
}

} // Citron::SyntaxIR0Translator