using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace TestTask.Features.Dogs.Data
{
    // DTO повторяют формат JSON:API ответа https://dogapi.dog/api/v2/breeds

    [Preserve]
    public sealed class BreedsResponseDto
    {
        [JsonProperty("data")] public BreedDataDto[] Data { get; set; }
    }

    [Preserve]
    public sealed class BreedResponseDto
    {
        [JsonProperty("data")] public BreedDataDto Data { get; set; }
    }

    [Preserve]
    public sealed class BreedDataDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("attributes")] public BreedAttributesDto Attributes { get; set; }
    }

    [Preserve]
    public sealed class BreedAttributesDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("hypoallergenic")] public bool Hypoallergenic { get; set; }
        [JsonProperty("life")] public RangeDto Life { get; set; }
    }

    [Preserve]
    public sealed class RangeDto
    {
        [JsonProperty("min")] public int Min { get; set; }
        [JsonProperty("max")] public int Max { get; set; }
    }
}
