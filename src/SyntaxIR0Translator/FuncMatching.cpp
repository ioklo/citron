#include "FuncMatching.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

optional<ArgumentsMatch> MatchArguments(
    RTypeArguments* outerTypeArgs, 
    RTypeArguments* partialTypeArgsExceptOuter, 
    vector<RFuncParameter>&& funcParams, 
    bool bVariadic, 
    SArguments* sArgs)
{
    throw NotImplementedException{};
}

} // namespace Citron