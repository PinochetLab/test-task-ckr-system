using UnityEngine;
using UnityEngine.UI;

namespace TestTask.Core.UI
{
    /// <summary>
    /// Заголовок экрана вкладки (префаб ScreenTitle).
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class ScreenTitleView : MonoBehaviour
    {
        [SerializeField] private Text _text;

        public string Title
        {
            get => _text.text;
            set => _text.text = value;
        }

        private void Reset()
        {
            _text = GetComponent<Text>();
        }
    }
}
