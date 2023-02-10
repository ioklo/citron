namespace Citron.Infra
{
    public interface IMutable<T> where T : class
    {
        T Clone(CloneContext context);

        // 필요 할때 다시 추가
        void Update(T src, UpdateContext context); 
    }
}