
add_library(QIrLLVMTranslator SHARED)

find_package(LLVM REQUIRED CONFIG)

message(STATUS "Found LLVM ${LLVM_PACKAGE_VERSION}")
message(STATUS "Using LLVMConfig.cmake in: ${LLVM_DIR}")

set_target_properties(QIrLLVMTranslator PROPERTIES DEFINE_SYMBOL QIRLLVMTRANSLATOR_EXPORTS)

target_compile_definitions(QIrLLVMTranslator PRIVATE ${LLVM_DEFINITIONS})

target_include_directories(QIrLLVMTranslator
        PRIVATE Public/QIrLLVMTranslator
        ${LLVM_INCLUDE_DIRS}
        PUBLIC Public)

llvm_map_components_to_libnames(LLVM_LIBS core support)

target_link_libraries(QIrLLVMTranslator PRIVATE
        Common
        Infra
        NSymbol
        QIR
        RSymbol
        ${LLVM_LIBS}
)

include(CMakeSources.g.txt)
