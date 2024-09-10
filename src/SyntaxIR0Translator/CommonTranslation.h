#pragma once
#include <vector>
#include <string>
#include <IR0/RDecl.h>

namespace Citron {

class STypeParam;

std::vector<std::string> MakeTypeParams(const std::vector<STypeParam>& typeParams);



}