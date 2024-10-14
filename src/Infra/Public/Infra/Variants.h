#pragma once

// code from https://en.cppreference.com/w/cpp/utility/variant/visit
template<class... Ts>
struct overloaded : Ts... { using Ts::operator()...; };

// explicit deduction guide (not needed as of C++20)
// template<class... Ts>
// overloaded(Ts...) -> overloaded<Ts...>;
