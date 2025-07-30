
add_executable(SyntaxPrinter)

target_compile_options(SyntaxPrinter PRIVATE
        -Wno-unqualified-std-cast-call
)

target_link_libraries(SyntaxPrinter PRIVATE Infra Syntax TextAnalysis)

target_sources(SyntaxPrinter
        PRIVATE
        SyntaxPrinter.cpp
)