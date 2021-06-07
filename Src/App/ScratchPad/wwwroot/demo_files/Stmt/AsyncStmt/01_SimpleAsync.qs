// 50504950

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