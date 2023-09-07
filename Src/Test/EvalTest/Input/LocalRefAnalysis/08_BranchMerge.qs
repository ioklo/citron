// $Error

void Main()
{
    lambda_value<int, int, 100> p;
    {
        int x = 3;
        {
            int y = 2;
            lambda_value<int, int, 100> q;

            if (true)
            {
                q = () => x;     // q: local-ref-contained(x)
            }
            else
            {
                q = () => y;     // q: local-ref-contained(y)
            }

            // q: local-ref-contained(x, y)
            p = q; // 에러, p의 범위가 x, y 범위보다 넓음 local-ref-contained(x, y)
        }
    }
}