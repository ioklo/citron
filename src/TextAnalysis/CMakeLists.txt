# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(TextAnalysis SHARED
  ../../include/TextAnalysis/TextAnalysisConfig.h

  ../../include/TextAnalysis/Buffer.h
  Buffer.cpp

  ../../include/TextAnalysis/BufferPosition.h
  BufferPosition.cpp

  ../../include/TextAnalysis/ExpParser.h
  ExpParser.cpp

  ../../include/TextAnalysis/Lexer.h
  Lexer.cpp

  ParserMisc.h
  ParserMisc.cpp 

  pch.h
  pch.cpp 

  ../../include/TextAnalysis/ScriptParser.h
  ScriptParser.cpp
  
  ../../include/TextAnalysis/TypeExpParser.h
  TypeExpParser.cpp

  ../../include/TextAnalysis/StmtParser.h
  StmtParser.cpp
)

target_compile_features(TextAnalysis PRIVATE cxx_std_20)

# TODO: Add tests and install targets if needed.
find_package(ICU REQUIRED COMPONENTS uc dt in io) # uc dt in io

target_include_directories(TextAnalysis 
  PUBLIC 
    ../../include
    ../../externals/utfcpp/source)
target_compile_definitions(TextAnalysis PRIVATE TEXT_ANALYSIS_EXPORT)
target_link_libraries(TextAnalysis PUBLIC Infra Syntax ICU::uc ICU::in ICU::dt ICU::io) 

target_compile_options(TextAnalysis PRIVATE "$<$<C_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(TextAnalysis PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(TextAnalysis PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/Zc:__cplusplus>")
