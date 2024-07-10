#pragma once

class MModuleDecl;
class MNamespaceDecl;

namespace Citron
{

using MTopLevelOuter = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>
>;

}