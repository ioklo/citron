
add_executable(DocUpdater)

find_package(Boost REQUIRED COMPONENTS algorithm)

target_sources(DocUpdater
    PRIVATE
        DocUpdater.cpp
    
    PRIVATE FILE_SET headers TYPE HEADERS FILES
)

target_link_libraries(DocUpdater PRIVATE
        Common
        Boost::algorithm
)