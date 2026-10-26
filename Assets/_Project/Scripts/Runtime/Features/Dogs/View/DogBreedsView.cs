using TestTask.Core.UI;
using TMPro;
using UnityEngine;

namespace TestTask.Features.Dogs.View
{
    public sealed class DogBreedsView : TabScreenView, IDogBreedsView
    {
        [SerializeField] private LoadingIndicatorView _loader;
        [SerializeField] private TMP_Text _errorText;

        public void SetLoading(bool loading)
        {
            _loader.SetVisible(loading);
        }

        public void SetError(string message)
        {
            _errorText.text = message;
        }
    }
}
