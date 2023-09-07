// 01234

int Add(int i)
{
    @$i
    return i + 1;
}

void Main()
{
    for(int i = 0; i < 5; i = Add(i));
}