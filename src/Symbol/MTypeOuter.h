#pragma once

namespace Citron
{

class MModuleDecl;
class MNamespaceDecl;
class MClass;
class MStruct;

using MTypeOuter = std::variant<
    std::shared_ptr<class MModuleDecl>,
    std::shared_ptr<class MNamespaceDecl>,
    std::shared_ptr<class MClass>,
    std::shared_ptr<class MStruct>
>;

}