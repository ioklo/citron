# launch ... with

# Syntax

```csharp
**launch** statement0 **with** statement1 **with** statement2 ...
**launch** (context) statement0 **with** statement1 **with** statement2 ...
```

# Remarks

주어진 구문을 병렬로 실행합니다. 구문 끝에서 대기 하지 않고 현재 실행을 이어 갑니다.

기본 런타임 설정을 context 인자로 변경할 수 있습니다 

식은 리턴값이 없는 task로 평가됩니다 

# Examples

```csharp
launch F(2, 3) with G();  // F(2, 3)과 G가 동시 실행
```

```csharp
await launch F(2, 3) with G(); // task로 평가되므로 대기가능
```

```csharp
string F()
{
    int sum = 0;
    string? text;

    var task = launch 
    {
        for (int i = 0; i < 10000; i++)
        {
            sum += i;
        }
    }
    with 
    {
        text = ReadFile("a.txt");
    }

    task.wait();
    return (text ?? "") + sum.ToString();
}
```