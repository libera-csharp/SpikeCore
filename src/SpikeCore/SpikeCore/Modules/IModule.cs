namespace SpikeCore.Modules
{
    public interface IModule
    {
        string Name { get; }
        string Description { get; }
        string Instructions { get; }
    }
}