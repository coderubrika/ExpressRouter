namespace Suburb.ExpressRouter
{
    public interface IRouterAnimation<T>
    {
        public ActItem<T> Animate { get; }
    }
}