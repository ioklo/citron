#include "BuildNonTypeSymbolContext.h"
#include <variant>

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"

#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypeParam.h"
#include "CommonTranslation.h"
#include "SmDeclContext.h"
#include "SmTypeRes.h"
#include "Misc.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {

BuildNonTypeSymbolContext::BuildNonTypeSymbolContext(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

expected<RFuncReturn, DiagPtr> BuildNonTypeSymbolContext::MakeFuncReturn(SFuncReturn& funcRet, RDecl* decl, std::span<RTypeParam*> typeParams, SmTypeResolveScope scope)
{
    SmTypeTranslationContexts contexts{scope, rFactory.get()};
    return Citron::MakeFuncReturn(funcRet, decl, typeParams, contexts);
}

expected<tuple<vector<RFuncParameter>, bool>, DiagPtr> BuildNonTypeSymbolContext::MakeFuncParameters(span<SFuncParam> sParams, SmTypeResolveScope scope)
{
    SmTypeTranslationContexts contexts{scope, rFactory.get()};
    return Citron::MakeFuncParameters(sParams, contexts);
}

}