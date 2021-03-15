# 함수에서 빠져나오기

```csharp
int Func1()
{
    // 1. 값과 함께 빠져나오기
    return 3; 
}

void Func2()
{    
} // 2. void함수는 끝에 도달하면 자동으로 빠져나온다

void Func3()
{
    return; // 3. void 전용 빈 return
}
```

