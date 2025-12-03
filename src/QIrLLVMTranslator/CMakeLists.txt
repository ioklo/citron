
add_library(QIrLLVMTranslator SHARED)

find_package(LLVM REQUIRED CONFIG)

message(STATUS "Found LLVM ${LLVM_PACKAGE_VERSION}")
message(STATUS "Using LLVMConfig.cmake in: ${LLVM_DIR}")

set_target_properties(QIrLLVMTranslator PROPERTIES DEFINE_SYMBOL QIRLLVMTRANSLATOR_EXPORTS)

target_compile_options(QIrLLVMTranslator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_compile_definitions(QIrLLVMTranslator PRIVATE ${LLVM_DEFINITIONS})

target_include_directories(QIrLLVMTranslator
        PRIVATE Public/QIrLLVMTranslator
        ${LLVM_INCLUDE_DIRS}
        PUBLIC Public)

target_link_libraries(QIrLLVMTranslator PRIVATE Infra NSymbol QIR RSymbol)

llvm_map_components_to_libnames(LLVM_LIBS core support)
target_link_libraries(QIrLLVMTranslator PRIVATE ${LLVM_LIBS})

target_sources(QIrLLVMTranslator
        PRIVATE
        QIrLLVMTranslator.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/QIrLLVMTranslator/QIrLLVMTranslator.h
        Public/QIrLLVMTranslator/QIrLLVMTranslatorConfig.h
)
