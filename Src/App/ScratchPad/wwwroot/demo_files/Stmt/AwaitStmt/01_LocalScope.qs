// 

void F()
{
    // await가 없으므로 기다리지 않는다
    async 
    {
        @yield
        @wrong
    }
}

await
{    
    F();
}
