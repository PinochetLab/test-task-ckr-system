using TMPro;
using UnityEngine;

namespace TestTask.Core.UI
{
    /// <summary>
    /// Заголовок экрана вкладки (префаб ScreenTitle).
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ScreenTitleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public string Title
        {
            get => _text.text;
            set => _text.text = value;
        }

        private void Reset()
        {
            _text = GetComponent<TMP_Text>();
        }
    }
}
