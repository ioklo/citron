#include <filesystem>
#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <format>
#include <algorithm>

using namespace std;
namespace fs = std::filesystem;


// vcxproj



// u8string
std::string ReadAll(fs::path filePath)
{
    ifstream ifs(filePath);
    ostringstream oss;
    oss << ifs.rdbuf();

    return oss.str();


    //ifs.seekg(0, ifs.end);
    //ifstream::pos_type length = ifs.tellg();
    //ifs.seekg(0, ifs.beg);

    //std::string s;
    //s.resize(length);

    //ifs.read(s.data(), length);

    // return s;
}

void WriteAll(fs::path filePath, string contents)
{
    if (fs::exists(filePath))
    {
        auto prevContents = ReadAll(filePath);
        if (contents == prevContents) return;
    }

    fs::create_directories(filePath.parent_path());

    ofstream ofs(filePath);
    ofs.write(contents.c_str(), contents.size());
    ofs.close();
}

void fill(vector<fs::path>& cppPaths, vector<fs::path>& hPaths, fs::path curPath, fs::path& basePath)
{
    size_t prevCppPathsSize = cppPaths.size();
    size_t prevHPathSize = hPaths.size();

    vector<fs::path> childPaths;
    for (auto& entry : fs::directory_iterator(curPath))
    {
        if (entry.is_directory())
        {
            childPaths.push_back(entry.path());
            continue;
        }

        auto filePath = entry.path();
        auto fileExt = filePath.extension();

        if (fileExt == ".cpp")
        {
            auto fileRelPath = fs::relative(filePath, basePath);
            cppPaths.push_back(fileRelPath);
        }
        else if (fileExt == ".h")
        {
            auto fileRelPath = fs::relative(filePath, basePath);
            hPaths.push_back(fileRelPath);
        }
        else continue;
    }

    sort(cppPaths.begin() + prevCppPathsSize, cppPaths.end());
    sort(hPaths.begin() + prevHPathSize, hPaths.end());
    sort(childPaths.begin(), childPaths.end());

    for (auto& childPath : childPaths)
    {
        fill(cppPaths, hPaths, childPath, basePath);
    }
}

int main(int argc, char* argv[])
{
    // git root directory를 인자로 준다
    // CMakeSourcesGenerator ../../../..

    if (argc < 2)
    {
        cout << "CMakeSourcesGenerator [git root path]" << endl;
        return 1;
    }

    auto basePath = fs::canonical(argv[1]);
    cout << "Root Directory: " << basePath << endl;

    // CMakeSources.g.txt를 적용할 프로젝트 (디렉토리)를 적어준다
    vector<fs::path> paths {
        "src/Infra",
        "src/Syntax",
        "src/Logging",
        "src/ESymbol",
        "src/RSymbol",
        "src/NSymbol",
        "src/MIR",
        "src/QIR",
        "src/TextAnalysis",
        "src/SyntaxIR0Translator",
        "src/IR0IR1Translator",
        "src/QIrLLVMTranslator",
        "src/QEvaluator",
        "src/Builder",
        "src/RuntimeLibrary",
        "src/TestConsole",
        "src/TextAnalysis.Tests",
        "src/Builder.Tests",
        "src/QEvaluator.Tests",
        "src/EvalTests",

        "src/Tools/SyntaxPrinter",
    };

    for (auto& filePath : paths)
    {
        auto projPath = basePath / filePath;
        if (!exists(projPath))
        {
            cout << projPath << " 가 없습니다" << endl;
            continue;
        }

        string projName = projPath.filename();
        cout << format("[{}]", projName) << endl;

        vector<fs::path> cppPaths, hPaths;
        fill(cppPaths, hPaths, projPath, projPath);

        ostringstream cppStream;
        for (auto& cppPath : cppPaths)
        {
            cppStream << "        " << cppPath.string() << endl;
        }

        ostringstream hStream;
        for (auto& hPath : hPaths)
        {
            hStream << "        " << hPath.string() << endl;
        }

        string contents = format(R"---(target_sources({}
    PRIVATE
{}
    PRIVATE FILE_SET headers TYPE HEADERS FILES
{}))---", projName, cppStream.str(), hStream.str());


        auto txtPath = projPath / "CMakeSources.g.txt";
        WriteAll(txtPath, contents);

// target_sources(CodeGenerator
//         PRIVATE
//         CodeGenerator.cpp
//         Misc.cpp
//         SyntaxCodeGenerator.cpp
//
//         PRIVATE FILE_SET headers TYPE HEADERS
//         FILES
//           Misc.h
//           SyntaxCodeGenerator.h
// )



    }

    return 0;
}