%%BEGIN_EMBED(Blank_Statement_For)%%
```cs
//@ 01234
int Add(int i)
{
    @$i
    return i + 1;
}

void Main()
{
    for(int i = 0; i < 5; i = Add(i));
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Blank_Statement_Foreach)%%
```cs
//@ helloworld1
seq string F()
{
    @hello
    yield "1";
    @world
    yield "2";
    @1
}

void Main()
{
    foreach(var i in F());
}

```
%%END_EMBED%%