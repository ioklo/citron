<!--BEGIN_EMBED(Return_Statement_ControlFlow)-->
```cs
//@ F
void F()
{    
    @F

    return;

    @wrong
}

void Main()
{
    F();
}

```
<!--END_EMBED-->

<!--BEGIN_EMBED(Return_Statement_ReturnValue)-->
```cs
//@ F6
int F(int i)
{    
    @F

    return i * 2;

    @wrong
}

void Main()
{
    @${F(3)}
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Return_Statement_LambdaReturn)-->
```cs
//@ 3
void Main()
{
    var f = () => {
        return 3;
    };

    @${f()}
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Return_Statement_SeqReturn)-->
```cs
//@ 01234
seq int F()
{
    for(int i = 0; i < 10; i++)
    {
        yield i;
        if (i == 4) return;
    }
}

void Main()
{
    foreach(var e in F())
    {
        @$e
    }
}
```
<!--END_EMBED-->