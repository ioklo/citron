module Citron.MDecls:MModule;

using namespace std;

namespace Citron {

MModule::MModule(string&& moduleName)
    : moduleName(std::move(moduleName))
{

}

}