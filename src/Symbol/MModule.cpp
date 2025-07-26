module;
#include <string>

module Citron.MDecls;

using namespace std;

namespace Citron {

MModule::MModule(string&& moduleName)
    : moduleName(move(moduleName))
{

}

}