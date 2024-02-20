
#find_package(ICU REQUIRED COMPONENTS uc dt in io)

add_library(Syntax
    ../../include/Syntax/Syntax.h
    ../../include/Syntax/SyntaxConfig.h
    ../../include/Syntax/Syntaxes.g.h
    ../../include/Syntax/Tokens.h

    pch.cpp
    pch.h

    Syntaxes.cpp
    Syntaxes.g.cpp
)

target_include_directories(Syntax PUBLIC ../../include)
target_link_libraries(Syntax)