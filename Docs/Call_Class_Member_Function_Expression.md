%%TEST(Basic, hi 2 4)%%
```
// 2 4
class X
{
    int x;

	public static void S()
	{
		@hi
	}

    public void F(int i)
    {
        @ $x $i
    }

    public X(int x) { this.x = x; }
}

void Main()
{
	X.S();
	
    X x = new X(2);
    x.F(4);
}
```