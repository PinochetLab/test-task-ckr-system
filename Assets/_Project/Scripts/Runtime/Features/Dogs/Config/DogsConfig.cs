using UnityEngine;

namespace TestTask.Features.Dogs.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Dogs", fileName = "DogsConfig")]
    public sealed class DogsConfig : ScriptableObject
    {
        [SerializeField] private string _baseUrl = "https://dogapi.dog/api/v2";
        [SerializeField, Min(1)] private int _breedsCount = 10;

        public string BaseUrl => _baseUrl.TrimEnd('/');
        public int BreedsCount => _breedsCount;
    }
}
