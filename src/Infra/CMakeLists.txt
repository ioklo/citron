
#find_package(ICU REQUIRED COMPONENTS uc dt in io)

add_library(Infra

    ../../include/Infra/IWriter.h
    ../../include/Infra/Indirect.h
    ../../include/Infra/InfraConfig.h
    ../../include/Infra/Unreachable.h
    ../../include/Infra/make_vector.h
    ../../include/Infra/Json.h
    ../../include/Infra/StringWriter.h

    Json.cpp

    StringWriter.cpp

    pch.cpp
    pch.h
)

target_include_directories(Infra PUBLIC ../../include)