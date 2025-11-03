
add_library(Syntax SHARED)

set_target_properties(Syntax PROPERTIES DEFINE_SYMBOL SYNTAX_EXPORTS)

target_compile_options(Syntax PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Syntax
        PRIVATE Public/Syntax
        INTERFACE Public)

target_link_libraries(Syntax PRIVATE Infra)


target_sources(Syntax
        PRIVATE
        SFactory.cpp
        Syntaxes.cpp
        Syntaxes.g.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Syntax/SFactory.h
        Public/Syntax/Syntax.h
        Public/Syntax/SyntaxConfig.h
        Public/Syntax/Syntaxes.g.h
        Public/Syntax/Tokens.h
)

