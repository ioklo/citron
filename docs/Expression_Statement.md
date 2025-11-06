%%TEST(AssignAllowed, )%%
```
void Main()
{
    int a = 0;
    a = 3 + 7;
}

```

%%TEST(IntLiteralAsTopLevelExp, $Error)%%
```
void Main()
{
	3; // error
}
```