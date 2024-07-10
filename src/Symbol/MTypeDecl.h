#pragma once

namespace Citron {

class MClassDecl;
class MStructDecl;
class MEnumDecl;
class MEnumElemDecl;
class MInterfaceDecl;
class MLambdaDecl;

using MTypeDecl = std::variant<
    MClassDecl,
    MStructDecl,
    MEnumDecl,
    MEnumElemDecl,
    MInterfaceDecl,
    MLambdaDecl
>;

}