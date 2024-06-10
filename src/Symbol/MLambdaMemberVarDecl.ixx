export module Citron.Symbols:MLambdaMemberVarDecl;

import <memory>;
import :MNames;
import :MType;

namespace Citron
{

class MLambdaDecl;

export class MLambdaMemberVarDecl
{
    std::weak_ptr<MLambdaDecl> lambda;
    MType type;
    MName name;
};

}