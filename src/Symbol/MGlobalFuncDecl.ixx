export module Citron.MSymbol:MGlobalFuncDecl;

import <vector>;
import <optional>;

import :MDecl;
import :MDeclVisitor;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MAccessor;
import :MNames;
import :MFuncReturn;
import :MFuncParameter;

namespace Citron {

export class MNamespaceDecl;

export class MGlobalFuncDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
{
    struct FuncReturnAndParams
    {
        MFuncReturn funcReturn;
        std::vector<MFuncParameter> parameters;
    };

    std::weak_ptr<MNamespaceDecl> outer;
    MAccessor accessor;
    MName name;
    std::vector<MName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}