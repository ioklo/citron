# Windows Build And Test

Status: draft current
Area: process, build, test
Keywords: Windows, Visual Studio, msbuild, tests, GoogleTest

## Dev Shell
Windows build/test requires Visual Studio Dev Shell setup.

Use this at the start of a non-interactive PowerShell session:

```powershell
Import-Module "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell 627cbcf5 -SkipAutomaticLocation -DevCmdArguments "-arch=x64 -host_arch=x64"
```

## Build One Project
From a project directory:

```powershell
msbuild .\MIR.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64
```

Project file name can be omitted when unambiguous:

```powershell
msbuild /t:Build /p:Configuration=Debug /p:Platform=x64
```

## Build Solution
From `src`:

```powershell
msbuild Citron.sln /t:Build /p:Configuration=Debug /p:Platform=x64
```

## Tests
Current test projects:
- `src/Builder.Tests`
- `src/QEvaluator.Tests`
- `src/TextAnalysis.Tests`

Build test project with `msbuild`, then run the generated GoogleTest executable.

Example:
```powershell
cd src\Builder.Tests
msbuild .\Builder.Tests.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64
.\x64\Debug\Builder.Tests.exe
```

Use `--gtest_filter` for focused tests.

```powershell
.\x64\Debug\Builder.Tests.exe --gtest_filter=MPrinter*
```

## Source Reference
- `ai/process/build-test-and-generation-windows.md`
