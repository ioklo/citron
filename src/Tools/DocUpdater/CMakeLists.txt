
add_executable(DocUpdater)

find_package(Boost REQUIRED COMPONENTS algorithm)

include(CMakeSources.g.txt)

target_link_libraries(DocUpdater PRIVATE
        Common
        Boost::algorithm
)