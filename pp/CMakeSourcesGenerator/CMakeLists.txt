# 각 프로젝트 별로 target_sources를 생성해준다
# 이 프로젝트만 예외로 target_sources를 직접 적는다

add_executable(CMakeSourcesGenerator)

target_sources(CMakeSourcesGenerator
    PRIVATE
        CMakeSourcesGenerator.cpp

    PRIVATE FILE_SET headers TYPE HEADERS
        FILES
)

target_link_libraries(CMakeSourcesGenerator PRIVATE Common)