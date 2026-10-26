namespace TestTask.Features.Dogs.Data
{
    /// <summary>Подробности о породе для поп-апа.</summary>
    public sealed class BreedDetails
    {
        public BreedDetails(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}
