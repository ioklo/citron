
add_executable(CodeGenerator)

find_package(fmt CONFIG REQUIRED)

include(CMakeSources.g.txt)

target_link_libraries(CodeGenerator PRIVATE
        Common
        fmt::fmt
)