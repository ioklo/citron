export module Citron.Symbols:MTypeDecl;

import "std.h";

namespace Citron {

export class MClassDecl;
export class MStructDecl;
export class MEnumDecl;
export class MEnumElemDecl;
export class MInterfaceDecl;
export class MLambdaDecl;

export using MTypeDecl = std::variant<
    MClassDecl,
    MStructDecl,
    MEnumDecl,
    MEnumElemDecl,
    MInterfaceDecl,
    MLambdaDecl
>;

}