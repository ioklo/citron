
add_executable(SyntaxPrinter)

target_link_libraries(SyntaxPrinter PRIVATE
        Common
        Infra
        Syntax
        TextAnalysis
)
target_sources(SyntaxPrinter
        PRIVATE
        SyntaxPrinter.cpp
)