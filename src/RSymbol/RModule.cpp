#include "RModule.h"

using namespace std;

namespace Citron {

RModule::RModule(RName&& name)
    : name{std::move(name)}
{
}

}