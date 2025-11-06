%%NOTTEST%%
```
true

false
```
bool형 값을 환경의 result에 넣습니다

%%TEST(Literal, true false)%%
```cs
void Main()
{
    bool t = true;
    bool f = false;
    @$t $f
}
```

[Boolean](Boolean.md)