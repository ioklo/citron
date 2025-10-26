
add_library(Infra SHARED)

set_target_properties(Infra PROPERTIES DEFINE_SYMBOL INFRA_EXPORTS)

target_compile_options(Infra PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Infra
        PRIVATE Public/Infra
        INTERFACE Public)

target_sources(Infra
  PRIVATE
    Json.cpp
    StringWriter.cpp
  PUBLIC FILE_SET headers
    TYPE HEADERS
    FILES
      Public/Infra/Defaults.h
      Public/Infra/InfraConfig.h
      Public/Infra/Hash.h
      Public/Infra/Copy.h
      Public/Infra/Exceptions.h
      Public/Infra/Expected.h
      Public/Infra/Indirect.h
      Public/Infra/IWriter.h
      Public/Infra/Json.h
      Public/Infra/make_vector.h
      Public/Infra/Ptr.h
      Public/Infra/StringWriter.h
      Public/Infra/Unreachable.h
      Public/Infra/Variants.h
)