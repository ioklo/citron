<!--BEGIN_EMBED(Continue_Statement_For)-->
```cs
//@ 135
void Main()
{
    for (int i = 0; i < 6; i++)
    {
        if (i % 2 == 0) continue;
        @$i
    }
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Continue_Statement_Foreach)-->
```cs
//@ 711
void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        if (e % 2 == 0) continue;
        @$e
    }
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Continue_Statement_NestedFor)-->
```cs
//@ 711711
void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            if (i % 2 == 0) continue;
            @$i
        }
    }
}
```
<!--END_EMBED-->