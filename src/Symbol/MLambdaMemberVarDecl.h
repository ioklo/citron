#pragma once

#include <memory>

#include "MDecl.h"
#include "MNames.h"
#include "MType.h"

namespace Citron
{

class MLambdaDecl;

class MLambdaMemberVarDecl
    : public MDecl
{
    std::weak_ptr<MLambdaDecl> lambda;
    MTypePtr type;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}