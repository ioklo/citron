export module Citron.Symbols:MTypeDecl;

import <variant>;

namespace Citron {

class MClassDecl;
class MStructDecl;
class MEnumDecl;
class MEnumElemDecl;
class MInterfaceDecl;
class MLambdaDecl;

export using MTypeDecl = std::variant<
    MClassDecl,
    MStructDecl,
    MEnumDecl,
    MEnumElemDecl,
    MInterfaceDecl,
    MLambdaDecl
>;

}