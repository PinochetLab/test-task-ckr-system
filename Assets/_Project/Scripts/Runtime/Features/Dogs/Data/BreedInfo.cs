namespace TestTask.Features.Dogs.Data
{
    /// <summary>Элемент списка пород.</summary>
    public sealed class BreedInfo
    {
        public BreedInfo(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }
}
