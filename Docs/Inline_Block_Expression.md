%%NOTTEST%%
```
inline { <stmt> ... }
```
- 리턴 타입은 본문의 return 타입으로 유추하거나, 대입할 대상의 타입으로 정해집니다.(변수 선언 타입, 함수 인자 타입, 대입 대상 타입)
- inline은 최상위로 쓰일 수 없습니다.

%%NOTTEST%%
```
InlineBlockExp([Stmt] body)
```

# General
인라인 블록은 함수 본문에서 값으로 바로 평가되는 블록입니다. 함수 호출 오버헤드가 생기지 않습니다.

%%TEST(Basic, 3)%%
```cs

void Main()
{
    int s = 2;
    int x = inline {
        return (s + 4) / 2;
    };

    @$x;
}

```

%%TEST(InferByReturnType, 3)%%
```cs
void Main()
{
	var x = inline { return 3; };
	@$x
}
```

%%TEST(InferByAssignTargetType, 3)%%
```cs
void Main()
{
	int x;
	x = inline { return 3; };
	@$x
}
```

%%TEST(InferByFunctionParameter, 3)%%
```cs

void F(int x)
{
	@$x
}

void Main()
{
	F(inline { return 3; });
}
```

%%TEST(DifferentReturnType, $Error)%%
```cs
class B { }
class C1 : B { }
class C2 : B { }

void Main()
{
	var x = inline {
		if (true) 
		{
			return new C1();
		}
		else 
		{
			return new C2();
		}
	};
}
```

%%TEST(HintTypeFirst, )%%
```cs
class B { }
class C1 : B { }
class C2 : B { }

void Main()
{
	B x = inline {
		if (true) 
		{
			return new C1();
		}
		else 
		{
			return new C2();
		}
	};
}
```

%%TEST(ShouldNotBeVoid, $Error)%%
```cs
void Main()
{
	var s = inline { };
}
```

%%TEST(TopLevelNotAllowed, $Error)%%
```cs
void Main()
{
	inline { @{hello} } 
}
```