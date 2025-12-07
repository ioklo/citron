
add_executable(CodeGenerator)

find_package(fmt CONFIG REQUIRED)

target_sources(CodeGenerator
        PRIVATE
        CodeGenerator.cpp
        Misc.cpp
        SyntaxCodeGenerator.cpp

        PRIVATE FILE_SET headers TYPE HEADERS
        FILES
          Misc.h
          SyntaxCodeGenerator.h
)

target_link_libraries(CodeGenerator PRIVATE
        Common
        fmt::fmt
)