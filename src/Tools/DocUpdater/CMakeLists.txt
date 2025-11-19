
add_executable(DocUpdater)

target_compile_options(DocUpdater PRIVATE
        -Wno-unqualified-std-cast-call
)

find_package(Boost REQUIRED COMPONENTS algorithm)

target_sources(DocUpdater
        PRIVATE
        DocUpdater.cpp
)

target_link_libraries(DocUpdater PRIVATE Boost::algorithm)