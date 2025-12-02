#include "FuncMatching.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

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