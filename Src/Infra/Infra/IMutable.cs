namespace Citron.Infra
{
    public interface IMutable<T> where T : class
    {
        T Clone(CloneContext context);
        void Update(T src, UpdateContext context);
    }
}