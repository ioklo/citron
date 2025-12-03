#pragma once

#if defined(_MSC_VER)
#if defined(QEVALUATOR_EXPORTS)
#define QEVALUATOR_API __declspec(dllexport)
#else
#define QEVALUATOR_API __declspec(dllimport)
#endif
#elif defined(__GNUC__) || defined(__clang__)
#if defined(QEVALUATOR_EXPORTS)
#define QEVALUATOR_API __attribute__((visibility ("default")))
#else
#define QEVALUATOR_API
#endif
#endif