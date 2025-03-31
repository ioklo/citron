export module Citron.MDecls:MGlobalFuncDecl;

import <vector>;
import <optional>;

import :MDecl;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MFuncReturn;
import :MFuncParameter;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron {

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