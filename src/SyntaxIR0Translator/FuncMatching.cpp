#include "FuncMatching.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

optional<ArgumentsMatch> MatchArguments(
    const RTypeArgumentsPtr& outerTypeArgs, 
    const RTypeArgumentsPtr& partialTypeArgsExceptOuter, 
    vector<RFuncParameter>&& funcParams, 
    bool bVariadic, 
    const SArgumentsPtr& sArgs)
{
    throw NotImplementedException();
}

} // namespace Citron