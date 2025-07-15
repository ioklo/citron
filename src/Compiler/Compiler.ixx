export module Citron.Compiler;

import "CompilerConfig.h";
import <memory>;
import Citron.IFileSystem;

namespace Citron {

export class Compiler
{
    std::unique_ptr<IFileSystem> fileSystem;

public:
    COMPILER_API Compiler(std::unique_ptr<IFileSystem>&& fileSystem);

};

}