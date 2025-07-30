
add_library(Syntax SHARED)

add_dependencies(Syntax Infra)

target_compile_options(Syntax PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Syntax
        PRIVATE Public/Syntax
        INTERFACE Public)

target_link_libraries(Syntax PRIVATE Infra)


target_sources(Syntax
        PRIVATE
        Syntaxes.cpp
        Syntaxes.g.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Syntax/Syntax.h
        Public/Syntax/SyntaxConfig.h
        Public/Syntax/Syntaxes.g.h
        Public/Syntax/Tokens.h
)

