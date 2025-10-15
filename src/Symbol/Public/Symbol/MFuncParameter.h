#pragma once

#include <string>

namespace Citron {

class MType;

class MFuncParameter
{
    bool bOut;
    MType* type;
    std::string name;
};


}
