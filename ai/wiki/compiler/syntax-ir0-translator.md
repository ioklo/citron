# SyntaxIR0Translator

Status: draft current
Area: compiler, translator
Keywords: SyntaxIR0Translator, MIR, IR0, RDecl, NDecl, PhaseManager

## Role
`SyntaxIR0Translator` converts syntax trees into MIR(IR0) and runtime declaration objects.

Main responsibilities:
- consume parser/TextAnalysis output;
- collect/register module-level declarations;
- create `NDecl` -> `RDecl` mapping;
- split work into declaration/function/struct tasks;
- translate expressions and statements into MIR nodes.

## Main Entry Points
- `src/SyntaxIR0Translator/PhaseManager.cpp`
- `src/SyntaxIR0Translator/GlobalContext.*`
- `src/SyntaxIR0Translator/FuncContext.*`

## High-Level Flow
1. `PhaseManager` receives syntax tree output from `TextAnalysis`.
2. Module-level declarations are collected by `ModuleDecls`.
3. `GlobalContext` registers declarations and establishes `NDecl` -> `RDecl` mapping.
4. Individual units are split into tasks such as `GlobalFuncTask`, `StructTask`.
5. Expressions/statements are translated by `SExpToMExpTranslation.*` family.
6. Intermediate expression forms include ImExp, ReExp, IrExp before MIR nodes.

## Code Search Tips
- Start with `GlobalFuncTask.cpp` to find where `NDecl` is registered as `RDecl`.
- Use `SExpToMExpTranslation.cpp` for concrete expression translation cases.
- Check `GlobalContext` / `FuncContext` for symbol and scope state.

## Testing
- Translator changes should add/update focused tests in `TextAnalysis.Tests` or relevant module tests.

## History
- `git history: ai/implementations/syntaxir0translator-implementation.md`
