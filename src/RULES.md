- ```std::expected```는 접두사 ```e_```를, ```std::optional```은 접두사 ```o_```를 붙인다
사용할때 ```*```로 참조해야할거라는걸 알게된다
```cs

std::expected<MLoc*, DiagPtr> GetMLoc();
std::optional<size_t> GetIndex();

auto e_mLoc = GetMLoc();
auto o_index = GetIndex(); 
```

- 접두어만 다른 리턴값을 가지는 interface는 Get다음에 접두어를 붙인다

```
GetBaseStruct는

NStructDecl* GetNBaseStruct(); // GetBaseNStruct하면 N을 어디에 붙일지 헷갈리기 시작한다 
RStructDecl* GetRBaseStruct(); // 무조건 Get다음에 붙이는 것으로
```