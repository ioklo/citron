#include "MModule.h"

using namespace std;

namespace Citron {


MModule::MModule(string&& moduleName)
    : moduleName(std::move(moduleName))
{

}

}