export module Citron.Symbols:MLambdaDecl;

import "std.h";
import :MBodyDeclOuter;
import :MNames;
import :MCommonFuncDeclComponent;
import :MLambdaMemberVarDecl;
import :MFuncReturn;

namespace Citron
{

export class MLambdaDecl : private MCommonFuncDeclComponent
{
    MBodyDeclOuter outer;
    MName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::vector<MLambdaMemberVarDecl> memberVars;

    // return은 확정이 안되었을 수 있다
    std::optional<MFuncReturn> oReturn;
};

}