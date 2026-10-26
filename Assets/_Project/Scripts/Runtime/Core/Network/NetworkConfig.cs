using UnityEngine;

namespace TestTask.Core.Network
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Network", fileName = "NetworkConfig")]
    public sealed class NetworkConfig : ScriptableObject
    {
        [Tooltip("api.weather.gov требует заголовок User-Agent")]
        [SerializeField] private string _userAgent = "TestTask-CKR-System/1.0";
        [SerializeField, Min(1)] private int _timeoutSeconds = 15;

        public string UserAgent => _userAgent;
        public int TimeoutSeconds => _timeoutSeconds;
    }
}
