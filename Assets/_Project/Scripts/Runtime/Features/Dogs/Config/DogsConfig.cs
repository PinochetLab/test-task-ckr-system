using UnityEngine;

namespace TestTask.Features.Dogs.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Dogs", fileName = "DogsConfig")]
    public sealed class DogsConfig : ScriptableObject
    {
        [SerializeField] private string _baseUrl = "https://dogapi.dog/api/v2";
        [SerializeField, Min(1)] private int _breedsCount = 10;

        [Header("Тексты")]
        [Tooltip("{0} — номер в списке, {1} — название породы")]
        [SerializeField] private string _itemFormat = "{0} - {1}";
        [SerializeField] private string _listErrorMessage = "Не удалось загрузить список пород";
        [SerializeField] private string _detailsErrorTitle = "Ошибка";
        [SerializeField] private string _detailsErrorMessage = "Не удалось загрузить описание породы";
        [SerializeField] private string _emptyDescription = "Описание отсутствует";

        public string BaseUrl => _baseUrl.TrimEnd('/');
        public int BreedsCount => _breedsCount;
        public string ItemFormat => _itemFormat;
        public string ListErrorMessage => _listErrorMessage;
        public string DetailsErrorTitle => _detailsErrorTitle;
        public string DetailsErrorMessage => _detailsErrorMessage;
        public string EmptyDescription => _emptyDescription;
    }
}
