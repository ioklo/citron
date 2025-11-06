%%BEGIN_EMBED(Block_Statement_Scope)%%
```cs
//@ 7
void Main()
{
    int a = 7;

    {
        int a = 0;
        a = 1;
    }

    @$a
}
```
%%END_EMBED%%