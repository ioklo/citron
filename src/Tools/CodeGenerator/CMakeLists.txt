
add_executable(CodeGenerator)

target_compile_options(CodeGenerator PRIVATE
        -Wno-unqualified-std-cast-call
)

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

target_link_libraries(CodeGenerator PRIVATE fmt::fmt)