#include "EModule.h"
#include <string>

using namespace std;

namespace Citron {

EModule::EModule(string&& moduleName)
    : moduleName(move(moduleName))
{

}

}