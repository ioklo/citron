Loc, ClassSymbol, Stmt

%%TEST(Basic, succeed)%%
```
class B { }
class C : B { }

void Main()
{
    B b = new C();
    if (C c = b)
    {
        @succeed
    }
}
```

%%TEST(CantTestValueType, $Error)%%
```
class C {}

void Main()
{
    var s1 = 0;
    if (s1 is C)  // wrong, 명확한 타입에 대해서는 타입비교 불가
        @false
}
```

