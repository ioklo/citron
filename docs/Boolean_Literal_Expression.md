%%NOTTEST%%
```
true

false
```
bool형 값을 환경의 result에 넣습니다

%%BEGIN_EMBED(Boolean_Literal_Expression_Literal)%%
```cs
void Main()
{
    bool t = true;
    bool f = false;
    @$t $f
}
```
%%END_EMBED%%

[Boolean](Boolean.md)