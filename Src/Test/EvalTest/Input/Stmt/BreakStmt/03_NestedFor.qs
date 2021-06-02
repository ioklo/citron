// 6767

for(int i = 0; i < 2; i++)
{
    foreach (int i in [6, 7, 1, 1, 4])
    {
        @$i
        if (i % 2 == 1) break;
    }
}