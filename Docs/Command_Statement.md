%%NOTTEST%%
```
@<string-body>[\r\n]+ // 한줄짜리
@{<string-body>}      // 범위 (여러 줄 가능)
```
string의 따옴표 안쪽과 같이 취급합니다. 

%%NOTTEST%%
```
CommandStmt([StringExp] stringExps)
```

%%TEST(Basic, hi)%%
```cs
void Main()
{
    @hi
}
```

%%TEST(Interpolated, abc177 abcworlddef false.84)%%
```cs
void Main()
{
    int i = 177;
    string s = "world";
    bool b = false;

    @abc$i abc${s}def $b.84
}
```

%%TEST(Block,          <- no ignore 8 blanks        hello world        good)%%
```cs
void Main()
{
    // plain, ignore blank lines, trailing blanks
    @{

        <- no ignore 8 blanks
        
        hello world

    }

    // with other statements
    if (true)
    @{
        good
    }
}
```

# Reference
[String_Expression](String_Expression.md)