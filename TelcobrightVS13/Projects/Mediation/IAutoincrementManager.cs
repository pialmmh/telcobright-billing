namespace TelcobrightMediation
{
    public interface IAutoIncrementManager
    {
        long GetNewCounter(AutoIncrementCounterType counterType);
        void WriteAllChanges();
    }

    public interface IAutoIncrementManager<T>:IAutoIncrementManager
    {
        T GetNewCounter();
    }
}