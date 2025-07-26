
add_library(Syntax SHARED)

target_compile_options(Syntax PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Syntax PUBLIC Public/Syntax)
target_link_libraries(Syntax PUBLIC Infra)

target_sources(Syntax
        PRIVATE
        Syntaxes.cpp
        Syntaxes.g.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Syntax/Syntax.h
        Public/Syntax/SyntaxConfig.h

        PUBLIC FILE_SET cxx_modules
        TYPE CXX_MODULES
        FILES

        Syntaxes.g.ixx
        Syntaxes.ixx
        Tokens.ixx
)