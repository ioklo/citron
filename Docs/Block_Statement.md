%%TEST(Scope, 7)%%
```cs
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