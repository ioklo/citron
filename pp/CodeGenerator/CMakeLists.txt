
add_executable(CodeGenerator)

target_sources(CodeGenerator
    PRIVATE
        CodeGenerator.cpp
        Misc.cpp
        SyntaxCodeGenerator.cpp
        VisitorGenerator.cpp

    PRIVATE FILE_SET headers TYPE HEADERS FILES
        Misc.h
        SyntaxCodeGenerator.h
        VisitorGenerator.h
)


target_link_libraries(CodeGenerator PRIVATE
    Common
)