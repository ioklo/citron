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

%%BEGIN_EMBED(Command_Statement_Basic)%%
```cs
void Main()
{
    @hi
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Command_Statement_Interpolated)%%
```cs
void Main()
{
    int i = 177;
    string s = "world";
    bool b = false;

    @abc$i abc${s}def $b.84
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Command_Statement_Block)%%
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
%%END_EMBED%%

# Reference
[String_Expression](String_Expression.md)