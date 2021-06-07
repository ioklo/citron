// 49505050

int sum = 0, sum2 = 0;

// 동시에 실행된다는걸 테스트하려면 Event객체를 만들어서 주고 받으면 될것 같다
await 
{
    task
    {
        for(int i = 0; i < 100; i++)
            sum = sum + i;                
    }

    task
    {
        for(int i = 0; i < 101; i++)
            sum2 = sum2 + i;                
    }
}

@$sum$sum2