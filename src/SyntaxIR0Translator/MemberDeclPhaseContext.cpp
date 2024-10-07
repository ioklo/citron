#include "pch.h"
#include "MemberDeclPhaseContext.h"

#include <Infra/Exceptions.h>
#include <IR0/RNames.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

tuple<vector<RFuncParameter>, bool> MemberDeclPhaseContext::MakeParameters(RDeclPtr decl, vector<SFuncParam>& sParams)
{
    bool bLastParamVariadic = false;

    size_t paramCount = sParams.size();
    vector<RFuncParameter> rParams;
    rParams.reserve(paramCount);

    for (size_t i = 0; i < paramCount; i++)
    {
        auto& sParam = sParams[i];

        auto type = this->MakeType(sParam.type, decl);
        if (!type) throw NotImplementedException(); // 에러 처리

        if (sParam.hasParams)
        {
            if (i == paramCount - 1)
            {
                bLastParamVariadic = true;
            }
            else
            {
                throw NotImplementedException(); // 에러 처리, params는 마지막 파라미터에만 사용할 수 있습니다
            }

        }

        rParams.push_back(RFuncParameter { sParam.hasOut, type, sParam.name });
    }

    return make_tuple(std::move(rParams), bLastParamVariadic);
}

}