```
inline { <stmt> ... }
```
- 리턴 타입은 본문의 return 타입으로 유추하거나, 대입할 대상의 타입으로 정해집니다.(변수 선언 타입, 함수 인자 타입, 대입 대상 타입)
- inline은 최상위로 쓰일 수 없습니다.

```
InlineBlockExp([Stmt] body)
```

# General
인라인 블록은 함수 본문에서 값으로 바로 평가되는 블록입니다. 함수 호출 오버헤드가 생기지 않습니다.

<!--BEGIN_EMBED(Inline_Block_Expression_Basic)-->
```cs
//@ 3
void Main()
{
    int s = 2;
    int x = inline {
        leave (s + 4) / 2;
    };

    @$x
}

```
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_InferByReturnType)-->
```cs
//@ 3
void Main()
{
	var x = inline { leave 3; };
	@$x
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_InferByAssignTargetType)-->
```cs
//@ 3
void Main()
{
	int x = uninit;
	x = inline { leave 3; };
	@$x
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_InferByFunctionParameter)-->
```cs
//@ hi3
void F(int x)
{
	@$x
}

void Main()
{
	F(inline { @{hi} 3 });
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_DifferentReturnType)-->
```cs
//@ $Error
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
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_HintTypeFirst)-->
```cs
//@ 
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
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_ShouldNotBeVoid)-->
```cs
//@ $Error
void Main()
{
	var s = inline { };
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Inline_Block_Expression_TopLevelNotAllowed)-->
```cs
//@ $Error
void Main()
{
	inline { @{hello} } 
}
```
<!--END_EMBED-->