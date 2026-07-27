#include "RModule.h"

using namespace std;

namespace Citron {

RModule::RModule(RModuleName&& name)
    : name{std::move(name)}
{
}

void RModule::FillIdentifier(std::string& buffer)
{
    buffer.append(name);
}

}