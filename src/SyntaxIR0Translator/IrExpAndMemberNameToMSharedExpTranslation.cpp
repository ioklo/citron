#include "IrExpAndMemberNameToMSharedExpTranslation.h"
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

expected<MSharedExp*, DiagPtr> TranslateIrExpAndMemberNameToMSharedExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
{
    throw NotImplementedException{};
}

} // namespace Citron