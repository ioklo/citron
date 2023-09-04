# Async

%%TEST(Async, 50504950)%%
```
void Main()
{
    int sum = 0, sum2 = 0;

    await 
    {
        async
        {
            // yield를 부름으로써 제어가 다음 async로 넘어간다
            @yield
    
            for(int i = 0; i < 100; i++)        
                sum = sum + i;
    
            @$sum
        }
    
        async
        {
            for(int i = 0; i < 101; i++)
                sum2 = sum2 + i;
    
            @$sum2
        }
    }
}
```

# Await
%%TEST(Await_LocalScope, )%%
```
void F()
{
    // await가 없으므로 기다리지 않는다
    async 
    {
        @yield
        @wrong
    }
}

void Main()
{
    await
    {    
        F();
    }
}
```