
%%TEST(Basic, 2)%%
```
class X
{
    public int x;
    public X(int x) { this.x = x; }
}

void Main()
{
    X x = new X(2);
    @${x.x}
}
```

%%TEST(Static, 11)%%
```
// 11
class C
{
    public static int x = 0;
    public void F()
    {
	    @$x
	}
}

void Main()
{
    C.x = 1;
    @${C.x}

	var c = new C();
	c.F();
}
```