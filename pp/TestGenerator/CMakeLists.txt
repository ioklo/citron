
add_executable(TestGenerator)

find_package(Boost REQUIRED COMPONENTS algorithm)

target_link_libraries(TestGenerator PRIVATE
        Common
        Boost::algorithm
)

target_sources(TestGenerator
    PRIVATE
        TestGenerator.cpp
    
    PRIVATE FILE_SET headers TYPE HEADERS FILES
)