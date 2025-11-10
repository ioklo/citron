#include "MLocQInstsTranslation.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron::IR0IR1Translator {

expected<QValue, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext)
{
    throw NotImplementedException{};
}

} // Citron