#pragma once

namespace Citron
{

class MModuleDecl;
class MNamespaceDecl;
class MClassDecl;
class MStructDecl;

// 보통 타입의 Outer
using MTypeDeclOuter = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>,
    std::weak_ptr<MClassDecl>,
    std::weak_ptr<MStructDecl>
>;

}