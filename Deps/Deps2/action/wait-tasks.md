# wait-tasks

WaitAllTasks인데.. 과연 WaitAnyTasks는 필요없는가

```csharp
void FuncAsync() // 내부에 await가 있으므로 async
{
    await
    {
        // 태스크 등록 
        task { ... }

        // 비동기 작업 등록
        async { ... } 

        // inline async 등록
        async DownloadAsync();

        // 이건 안 등록, 이렇게 하면 다운로드 끝날때까지 다음으로 안넘어간다
        DownloadAsync(); 
    }
}
```

