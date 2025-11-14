#include "BuildTypeDependentSymbolContext.h"

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NDecl.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

BuildTypeDependentSymbolContext::BuildTypeDependentSymbolContext(const NFactoryPtr& nFactory)
    : nFactory{nFactory}
{
}


RType* BuildTypeDependentSymbolContext::MakeType(STypeExp* sTypeExp, NDecl* decl)
{
    throw NotImplementedException{};
}

tuple<vector<RFuncParameter>, bool> BuildTypeDependentSymbolContext::MakeParameters(NDecl* decl, vector<SFuncParam>& sParams)
{
    bool bLastParamVariadic = false;

    size_t paramCount = sParams.size();
    vector<RFuncParameter> rParams;
    rParams.reserve(paramCount);

    for (size_t i = 0; i < paramCount; i++)
    {
        auto& sParam = sParams[i];

        auto type = this->MakeType(sParam.type, decl);
        if (!type) throw NotImplementedException{}; // 에러 처리

        if (sParam.hasParams)
        {
            if (i == paramCount - 1)
            {
                bLastParamVariadic = true;
            }
            else
            {
                throw NotImplementedException{}; // 에러 처리, params는 마지막 파라미터에만 사용할 수 있습니다
            }

        }

        rParams.emplace_back(sParam.hasOut, type, RName_Normal{sParam.name});
    }

    return make_tuple(move(rParams), bLastParamVariadic);
}

}