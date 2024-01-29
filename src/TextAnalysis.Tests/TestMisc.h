#pragma once

#include <tuple>
#include <memory>
#include <string>

#include <TextAnalysis/Lexer.h>

namespace Citron {
class Buffer;
class Lexer;
}

std::tuple<std::shared_ptr<Citron::Buffer>, Citron::Lexer> Prepare(std::u32string str);