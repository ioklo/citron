```
id(exp, params exp)
```

```
CallGlobalFuncExp(GlobalFuncSymbol symbol, [Argument] args)
```

<!--BEGIN_EMBED(Call_Global_Function_Expression_General)-->
```cs
//@ 1 2 false
void F(int i, string s, bool b)
{    
    @$i $s $b
}

void Main()
{
	F(1, "2", false);
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Call_Global_Function_Expression_Recursive)-->
```cs
//@ 345
void F(int i, int end)
{    
    if (end <= i) return;

    @$i
    F(i + 1, end);
}

void Main()
{
	F(3, 6);
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Call_Global_Function_Expression_Generator)-->
```cs
//@ 123
seq int Func()
{
    yield 1;
    yield 2;
    yield 3;
}

void Main()
{
    foreach(var i in Func())
        @$i
}
```
<!--END_EMBED-->

# Reference
[Global_Function](Global_Function.md)
[Argument](Argument.md)