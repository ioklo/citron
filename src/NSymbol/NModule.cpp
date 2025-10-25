#include "NModule.h"

#include <cassert>

using namespace std;

namespace Citron {

NModule::NModule(string&& name)
    : name{move(name)}
{
}

}