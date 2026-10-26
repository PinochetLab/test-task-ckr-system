namespace TestTask.Core.Network
{
    public interface IJsonParser
    {
        T Parse<T>(string json);
    }
}
