#include "IR0IR1Translator.h"

#include "IR0/NModule.h"
#include "IR1/QFactory.h"
#include "Logging/Diag.h"

using namespace std;

namespace Citron {

// L M N O P Q R S T U

// Module M
// IR0 R, N
// IR1 Q 만으로 해보자



// body만 바꾸면 되는데
QModule* Translate(NModule* nModule, DiagPtr diag, QFactory* factory)
{
    // nModule에는 NBody가 들어있다. 그 body만 변환해서 QModule로 만들어서 돌려주기만 하면 된다



    return nullptr;
}

} // namespace Citron