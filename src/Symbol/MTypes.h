#pragma once

#include <string>

#include "MType.h"
#include "MFuncReturn.h"
#include "MFuncParameter.h"

namespace Citron
{

// trivial types
class MTypeVarType
{
    int index;
    std::string name;
};

class MVoidType
{
};

class MSymbolType
{
};

// recursive types
class MNullableType
{
    MType innerType;
};

class MTupleMemberVar
{
    MType declType;
    std::string name;
};

class MTupleType
{
    std::vector<MTupleMemberVar> memberVars;
};

class MFuncType
{
    bool bLocal;
    MFuncReturn funcRet;
    std::vector<MFuncParameter> parameters;
};

class MLocalPtrType
{
    MType innerType;
};

class MBoxPtrType
{
    MType innerType;
};

}