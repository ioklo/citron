# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.

# Add source to this project's executable.
add_library (Infra
  StringWriter.cpp
  pch.cpp pch.h
  json.cpp
  exports.cpp

  ../../include/Infra/InfraConfig.h
  ../../include/Infra/Unreachable.h
  ../../include/Infra/StringWriter.h
  ../../include/Infra/make_vector.h
  ../../include/Infra/Json.h
  ../../include/Infra/IWriter.h
  ../../include/Infra/Indirect.h
)

# TODO: Add tests and install targets if needed.
target_include_directories (Infra PUBLIC ../../include)
target_compile_definitions(Infra PRIVATE INFRA_EXPORT)

