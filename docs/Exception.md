다음으로 미룹니다. 기본적으로는 함수 하나당 한 Exception만 던질수 있게 하기. Syntax는 C++이랑 비슷하게 가는게 맞지 않나

%%NOTTEST%%
```
enum FResult { One, Two, Three }

void F(int i) throws FResult
{
	if (i == 3)
		throw FResult.One;
}

// 1. exception passthrough
void G() throws FResult
{
	F();
}

// 2. try catch
void H() // nothrow
{
	try
	{
		G(); // exception 발생시
		...  // 건너뛰기
	}
	catch(FResult r)
	{
		...
	}
}



```