- ```std::expected```는 접두사 ```e_```를, ```std::optional```은 접두사 ```o_```를 붙인다
사용할때 ```*```로 참조해야할거라는걸 알게된다
```cs

std::expected<MLoc*, DiagPtr> GetMLoc();
std::optional<size_t> GetIndex();

auto e_mLoc = GetMLoc();
auto o_index = GetIndex(); 
```
- 