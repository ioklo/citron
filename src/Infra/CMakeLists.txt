
add_library(Infra SHARED)

target_include_directories(Infra PUBLIC Public/Infra)

target_precompile_headers(Infra PUBLIC
  <memory>
  <type_traits>
  <vector>
  <string>
  <expected>
  <optional>
)

target_sources(Infra
  PRIVATE
    Json.cpp
    StringWriter.cpp
  PUBLIC FILE_SET headers
    TYPE HEADERS
    FILES
      Public/Infra/Defaults.h
      Public/Infra/InfraConfig.h
  PUBLIC FILE_SET cxx_modules
    TYPE CXX_MODULES
    FILES
      Copy.ixx
      Exceptions.ixx
      Expected.ixx
      Hash.ixx
      Indirect.ixx
      IWriter.ixx
      Json.ixx
      make_vector.ixx
      Ptr.ixx
      StringWriter.ixx
      Unreachable.ixx
      Variants.ixx
)