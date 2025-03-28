export module Citron.MSymbol:MDeclId;

import "SymbolConfig.h";

import <string>;
import <memory>;

import :ForwardDecls;
import :MIdentifier;

namespace Citron {

export class MDeclPath
{
    std::shared_ptr<MDeclPath> outer;
    MIdentifier identifier;

private:
    MDeclPath(std::shared_ptr<MDeclPath>&& outer, MIdentifier&& identifier);
    friend MDeclIdFactory;
};

export using MDeclPathPtr = std::shared_ptr<MDeclPath>;

export class MDeclId
{
    std::string moduleName;
    MDeclPathPtr path;

private:
    MDeclId(std::string&& moduleName, MDeclPathPtr&& path);
    friend MDeclIdFactory;
};

export using MDeclIdPtr = std::shared_ptr<MDeclId>;

// flyweight
export class MDeclIdFactory
{
public:
    SYMBOL_API MDeclIdPtr GetBool();
    SYMBOL_API MDeclIdPtr GetInt();
    SYMBOL_API MDeclIdPtr GetString();

    SYMBOL_API MDeclIdPtr Get(std::string&& moduleName, MIdentifier&& identifier);
    SYMBOL_API MDeclIdPtr GetChild(MDeclIdPtr&& id, MIdentifier&& identifier);
};

} // namespace Citron
