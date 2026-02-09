
add_executable(SyntaxPrinter)

target_link_libraries(SyntaxPrinter PRIVATE
        Common
        Infra
        Syntax
        TextAnalysis
)

include(CMakeSources.g.txt)