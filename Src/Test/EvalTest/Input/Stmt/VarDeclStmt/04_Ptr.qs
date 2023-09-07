// 4

void Main()
{
    int i = 0;
    var x = 3;
    var* y = &i;

    int* s1 = &x, p;
    p = y;
    *p = 4;

    @${*y}
}