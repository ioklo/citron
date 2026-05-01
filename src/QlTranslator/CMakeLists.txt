
add_library(QlTranslator SHARED)

find_package(LLVM REQUIRED CONFIG)

message(STATUS "Found LLVM ${LLVM_PACKAGE_VERSION}")
message(STATUS "Using LLVMConfig.cmake in: ${LLVM_DIR}")

set_target_properties(QlTranslator PROPERTIES DEFINE_SYMBOL QLTRANSLATOR_EXPORTS)

target_compile_definitions(QlTranslator PRIVATE ${LLVM_DEFINITIONS})

target_include_directories(QlTranslator
        PRIVATE Public/QlTranslator
        ${LLVM_INCLUDE_DIRS}
        PUBLIC Public)

llvm_map_components_to_libnames(LLVM_LIBS core support)

target_link_libraries(QlTranslator PRIVATE
        Common
        Infra
        NSymbol
        QIR
        RSymbol
        ${LLVM_LIBS}
)

include(CMakeSources.g.txt)
