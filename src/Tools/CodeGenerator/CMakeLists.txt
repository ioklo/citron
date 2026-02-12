
add_executable(CodeGenerator)

include(CMakeSources.g.txt)

target_link_libraries(CodeGenerator PRIVATE
        Common
)