#include "MModule.h"
#include <string>

using namespace std;

namespace Citron {

MModule::MModule(string&& moduleName)
    : moduleName(move(moduleName))
{

}

}