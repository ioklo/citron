export module Citron.Symbols:MTypeDeclOuter;

import "std.h";

namespace Citron
{

export class MModuleDecl;
export class MNamespaceDecl;
export class MClassDecl;
export class MStructDecl;

// 보통 타입의 Outer
using MTypeDeclOuter = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>,
    std::weak_ptr<MClassDecl>,
    std::weak_ptr<MStructDecl>
>;

}