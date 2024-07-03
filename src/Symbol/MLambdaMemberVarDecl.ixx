export module Citron.Symbols:MLambdaMemberVarDecl;

import "std.h";

import :MNames;
import :MType;

namespace Citron
{

export class MLambdaDecl;

export class MLambdaMemberVarDecl
{
    std::weak_ptr<MLambdaDecl> lambda;
    MType type;
    MName name;
};

}