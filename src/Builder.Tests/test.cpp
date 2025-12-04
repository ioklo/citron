#include <gtest/gtest.h>

#include "Builder/Builder.h"
#include "Builder/IFileSystem.h"

using namespace std;
using namespace Citron;

class InMemoryFileSystem : public IFileSystem
{
    std::map<std::filesystem::path, std::vector<std::byte>> data;

public:
    void AddFile(const std::filesystem::path& path, const std::string& text)
    {
        std::vector<std::byte> buffer(text.size()); // without null

        std::transform(text.begin(), text.end(), buffer.begin(),
                   [](char c) { return std::byte(c); });

        data.emplace(path, std::move(buffer));
    }

    virtual std::optional<std::vector<std::byte>> GetFileContents(const std::filesystem::path& path) override
    {
        auto i = data.find(path);
        if (i == data.end()) return std::nullopt;

        return i->second;
    }
};

TEST(Builder, TestName)
{
    // 파일 구조 추가
    auto fs = make_unique<InMemoryFileSystem>();
    fs->AddFile("a.ct", R"---(
int main()
{
    print(1);
}
)---");

    Builder builder{std::move(fs)};

    // a.ct, b.ct 
    // compiler.Compile("a.ct", "b.ct");
    // 입력은 두개, 아래와 같은 일을 하게 된다

    // a.ct로 skeleton을 만든다 혹시 b.cti가 있다면 b.ct대신 skel을 만들때 b.cti를 쓴다
    auto skel = Skeletonize("a.ct", "b.ct", "myapp.skel");
    // 중간 파일인 "a.ct.skel", "b.ct.skel"이 생성되고, 최신버전이면 이것들로 부터 읽어들인다
    
    // 즉, 변경된 a.ct하나를 컴파일 하는데도 다른 변경된 ct파일을 Skel까진 해야한다

    // a.ct를 컴파일 하고, 그 결과를 a.ct.obj에 저장한다
    auto aObj = Compile(skel, "a.ct", "a.ct.obj"); // 
    auto bObj = Compile(skel, "b.ct", "b.ct.obj");

    // link는 다음에
    Link(aObj, bObj, executable);

    EXPECT_EQ(1, 1);
    EXPECT_TRUE(true);
}