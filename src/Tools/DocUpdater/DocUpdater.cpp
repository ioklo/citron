#include <iostream>
#include <locale>
#include <filesystem>
#include <fstream>
#include <sstream>
#include <codecvt>
#include <regex>
#include <cassert>

#include <boost/algorithm/string.hpp>

#ifdef _MSC_VER
#include <Windows.h>
#endif

using namespace std;
using namespace std::filesystem;

#ifdef _MSC_VER

std::wstring string_to_wide_string(std::string_view view)
{
    if (view.empty())
    {
        return L"";
    }

    const auto size_needed = MultiByteToWideChar(CP_UTF8, 0, view.data(), (int)view.size(), nullptr, 0);
    if (size_needed <= 0)
    {
        throw std::runtime_error("MultiByteToWideChar() failed: " + std::to_string(size_needed));
    }

    std::wstring result(size_needed, 0);
    MultiByteToWideChar(CP_UTF8, 0, view.data(), (int)view.size(), result.data(), size_needed);
    return result;
}
#else

#include <cwchar>

std::wstring string_to_wide_string(std::string_view view)
{
    mbstate_t state{};
    const char* str = view.data();
    size_t len = 1 + mbsrtowcs(nullptr, &str, 0, &state);

    vector<wchar_t> wstr(len);
    mbsrtowcs(&wstr[0], &str, wstr.size(), &state);

    return wstr.data();
}

#endif

std::string readAll(path filePath)
{
    ifstream ifs(filePath);
    ostringstream oss;
    oss << ifs.rdbuf();
    return oss.str();
}

void writeAll(path filePath, const string& str)
{
    ofstream ofs(filePath);
    istringstream iss(str);
    ofs << iss.rdbuf();
}

bool Update(path basePath, path docPath)
{
    auto text = readAll(docPath);
    size_t pos = 0;
    
    optional<string> embed;
    ostringstream oss;

    //             12             3       3  2 4         41
    regex r{R"(<!--((BEGIN_EMBED\(([^\\)]+)\))|(END_EMBED))-->)"};
    
    auto end = text.cend();
    while(true)
    {   
        smatch m;
        if (!regex_search(text.cbegin() + pos, end, m, r))
        {
            oss << string_view{text.data() + pos};
            break;
        }
        auto prefixLength = m.prefix().length();

        if (m[2].matched) // BEGIN
        {
            if (embed) return false; // 에러
            
            // BEGIN 포함해서 복사
            oss << string_view{text.data() + pos, (size_t)prefixLength + m.length(0)};
            embed.emplace(m[3].str());
        }
        else if (m[4].matched) // END
        {
            if (!embed) return false; // 에러

            // 파일 복사
            auto codePath = basePath / "data" / "TestData" / "EvalTests" / (*embed + ".ct");

            if (!exists(codePath))
            {
                wcout << L"Code file not found: " << codePath << endl;
                oss << endl << "<!--END_EMBED-->" << endl;
            }
            else
            {

                auto code = readAll(codePath);

                if (!code.ends_with("\n"))
                {
                    oss << endl << "```cs" << endl << code << endl << "```" << endl;
                }
                else
                {
                    oss << endl << "```cs" << endl << code << "```" << endl;
                }
                
                oss << string_view{text.data() + pos + prefixLength, (size_t)m.length(0)};
            }

            embed.reset();
        }
        else // ??
        {
            assert(false);
        }

        pos += prefixLength + m.length();
    }

    // 아직 embed가 남아있으면 에러
    if (embed)
    {
        wcout << L"BEGIN_EMBED not matched " << endl;
        return false;
    }

    // wcout << string_to_wide_string(oss.str());
    writeAll(docPath, oss.str());

    return true;
}

int wmain(int argc, wchar_t* argv[])
{
    locale::global(locale(""));

    if (argc < 2)
    {
        wcout << L"Usage: " << argv[0] << ' ' << L"[Base Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L"..\\..\\..\\ (relative to working directory, git root)" << endl;
        return 1;
    }

    auto basePath = absolute(argv[1]);
    wcout << L"Base Directory: " << basePath << endl;

    // 모든 md를 가져온다
    auto docsPath = basePath / "docs";

    // ".md"
    wstring mdExt = L".md";
    for (auto& entry : recursive_directory_iterator(docsPath))
    {
        wcout << entry << endl;

        auto filePath = entry.path();
        auto filename = filePath.wstring();
        if (filename.length() <= mdExt.size()) continue;

        size_t lengthWithoutExt = filename.length() - mdExt.size();
        if (!boost::iequals(wstring_view(filename).substr(lengthWithoutExt), mdExt)) continue;

        if (!Update(basePath, filePath))
            break;
    }

    return 0;
}


#if defined(__clang__)
int main(int argc, char* argv[])
{
    vector<std::wstring> wsargvs;
    wsargvs.reserve(argc);

    setlocale(LC_ALL, "");

    for(int i = 0; i < argc; i++)
    {
        size_t requiredSize = mbstowcs(nullptr, argv[i], 0);
        if (requiredSize == (size_t)-1)
            return 1;

        wsargvs.emplace_back(requiredSize, L' ');
        size_t ret = mbstowcs(wsargvs.back().data(), argv[i], requiredSize + 1);
        if (ret == (size_t)-1)
            return 1;
    }

    vector<wchar_t*> wargvs;
    wargvs.reserve(argc);
    for(int i = 0; i < argc; i++)
        wargvs.push_back(wsargvs[i].data());

    return wmain(argc, wargvs.data());
}
#endif

