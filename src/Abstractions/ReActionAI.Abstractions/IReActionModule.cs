namespace ReActionAI.Abstractions
{
    public interface IReActionModule
    {
        string Name { get; }
        string Version { get; }
        void Initialize();
    }
}