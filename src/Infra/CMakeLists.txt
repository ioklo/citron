

add_library(Infra

    PUBLIC
        ../../include/Infra/IWriter.h
        ../../include/Infra/Indirect.h
        ../../include/Infra/InfraConfig.h
        ../../include/Infra/Unreachable.h
        ../../include/Infra/make_vector.h
        ../../include/Infra/Json.h
        ../../include/Infra/StringWriter.h

    PRIVATE
        Json.cpp
        StringWriter.cpp

        pch.cpp
        pch.h
)

target_link_libraries()
target_compile_features()