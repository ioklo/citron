StringExp

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