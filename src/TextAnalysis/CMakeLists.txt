#find_package(ICU REQUIRED COMPONENTS uc dt in io)
find_package(ICU REQUIRED COMPONENTS dt)

add_library(TextAnalysis
	../../include/TextAnalysis/TextAnalysisConfig.h

	pch.h
	pch.cpp
	
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

	../../include/TextAnalysis/ScriptParser.h
	ScriptParser.cpp

	../../include/TextAnalysis/StmtParser.h
	StmtParser.cpp

	../../include/TextAnalysis/TypeExpParser.h
	TypeExpParser.cpp
)

target_include_directories(TextAnalysis 
	PUBLIC ../../include 
	PRIVATE ../../externals/utfcpp/source)

target_link_libraries(TextAnalysis
	PRIVATE Syntax ICU::dt)