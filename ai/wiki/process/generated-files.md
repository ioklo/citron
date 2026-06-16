# Generated Files

Status: draft current
Area: process, generation
Keywords: generated files, pp, CodeGenerator, .g.cpp, .g.h

## Rules
- `.g.cpp` and `.g.h` files are generated files.
- Do not edit `.g.cpp` / `.g.h` directly.
- Change generator source under `pp`, run generator, and include generated results in the repo.
- Generated files are not ignored; review and commit them when they are part of the change.

## Generator
The `pp` directory usually handles preprocess/code generation.

Typical generator invocation from the generator output directory:

```powershell
CodeGenerator.exe ..\..\..
```

The argument is the git root path. Generated files under `src` are updated relative to that root.

## Source Reference
- `ai/process/build-test-and-generation-windows.md`
