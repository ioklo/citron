add_executable(TestConsole)

target_include_directories(TestConsole PRIVATE .)


target_link_libraries(TestConsole PRIVATE
        Common
)

target_sources(TestConsole
        PRIVATE
        TestConsole.cpp
        pch.cpp
)
