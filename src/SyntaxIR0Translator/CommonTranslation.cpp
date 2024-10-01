#include "pch.h"
#include "CommonTranslation.h"
#include <Syntax/Syntax.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

vector<string> MakeTypeParams(const vector<STypeParam>& sTypeParams)
{
    vector<string> result;
    result.reserve(sTypeParams.size());

    for (auto& sTypeParam : sTypeParams)
        result.push_back(sTypeParam.name);

    return result;
}

}
