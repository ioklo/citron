add_executable(TestConsole)

target_include_directories(TestConsole PRIVATE .)


target_link_libraries(TestConsole PRIVATE
        Common
)

include(CMakeSources.g.txt)