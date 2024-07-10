# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(TextAnalysis
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

# TODO: Add tests and install targets if needed.
find_package(ICU REQUIRED COMPONENTS uc dt in io) # uc dt in io

target_include_directories(TextAnalysis 
  PUBLIC 
    ../../include
    ../../externals/utfcpp/source)
target_compile_definitions(TextAnalysis 
  PRIVATE 
    TEXTANALYSIS_EXPORT
  PUBLIC
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
    $<$<STREQUAL:$<TARGET_PROPERTY:Syntax,TYPE>,SHARED_LIBRARY>:SYNTAX_IMPORT>
)

target_link_libraries(TextAnalysis PUBLIC Infra Syntax ICU::uc ICU::in ICU::dt ICU::io) 
