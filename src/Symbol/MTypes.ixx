export module Citron.Symbols:MTypes;

import "std.h";

export import :MType;
import :MFuncReturn;
import :MFuncParameter;

namespace Citron
{

// define incomplete types
export class MNullableType
{
    MType innerType;
};

export class MTupleMemberVar
{
    MType declType;
    std::string name;
};

export class MTupleType
{
    std::vector<MTupleMemberVar> memberVars;
};

export class MFuncType
{
    bool bLocal;
    MFuncReturn funcRet;
    std::vector<MFuncParameter> parameters;
};

export class MLocalPtrType
{
    MType innerType;
};

export class MBoxPtrType
{
    MType innerType;
};

}