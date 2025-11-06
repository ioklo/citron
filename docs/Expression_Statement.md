%%BEGIN_EMBED(Expression_Statement_AssignAllowed)%%
```
void Main()
{
    int a = 0;
    a = 3 + 7;
}

```
%%END_EMBED%%

%%BEGIN_EMBED(Expression_Statement_IntLiteralAsTopLevelExp)%%
```
void Main()
{
	3; // error
}
```
%%END_EMBED%%