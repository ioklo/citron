add_library(Common INTERFACE)

target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:Clang>:-Wno-unqualified-std-cast-call>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:AppleClang>:-Wno-unqualified-std-cast-call>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:MSVC>:/utf-8>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:MSVC>:/Zc:__cplusplus>)
