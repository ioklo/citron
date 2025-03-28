export module Citron.MSymbol:MInterfaceDecl;

import <vector>;

import :MDecl;
import :MTypeDecl;
import :MTypeDeclOuter;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{

export class MInterfaceDecl
    : public MDecl
    , public MTypeDecl
{
    MTypeDeclOuterWPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}