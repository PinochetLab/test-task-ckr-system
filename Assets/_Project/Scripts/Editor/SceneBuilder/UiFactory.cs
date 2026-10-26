using TestTask.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.EditorTools.SceneBuilder
{
    /// <summary>
    /// Помощники для создания простого UI из цветных прямоугольников и стандартного шрифта.
    /// </summary>
    internal static class UiFactory
    {
        private static Font _font;

        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                return _font;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");

            var rect = (RectTransform)go.transform;
            if (parent != null)
                rect.SetParent(parent, false);

            return rect;
        }

        public static RectTransform Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        /// <summary>Растягивает по родителю с относительным отступом (доля размера) с каждой стороны.</summary>
        public static RectTransform StretchRelative(RectTransform rect, float inset)
        {
            rect.anchorMin = new Vector2(inset, inset);
            rect.anchorMax = new Vector2(1f - inset, 1f - inset);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform PlaceTop(RectTransform rect, float y, Vector2 size)
        {
            return Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), size);
        }

        public static RectTransform PlaceCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            return Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        }

        public static Image AddImage(RectTransform rect, Color color, bool raycastTarget = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Image CreateImage(string name, Transform parent, Color color, bool raycastTarget = false)
        {
            return AddImage(CreateRect(name, parent), color, raycastTarget);
        }

        public static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            FontStyle style = FontStyle.Normal)
        {
            RectTransform rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button AddButton(Image targetGraphic, bool tint = true)
        {
            var button = targetGraphic.gameObject.AddComponent<Button>();
            targetGraphic.raycastTarget = true;
            button.targetGraphic = targetGraphic;

            if (tint)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f);
                colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
                colors.selectedColor = Color.white;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                colors.fadeDuration = 0.05f;
                button.colors = colors;
            }
            else
            {
                button.transition = Selectable.Transition.None;
            }

            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            return button;
        }

        public static Button CreateButton(string name, Transform parent, Color color, string label, int fontSize, Color labelColor, out Text labelText)
        {
            Image image = CreateImage(name, parent, color, true);
            Button button = AddButton(image);

            labelText = CreateText("Label", image.transform, label, fontSize, labelColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(labelText.rectTransform, 12f, 12f, 8f, 8f);
            return button;
        }

        /// <summary>Индикатор загрузки: вращающийся квадрат с рамкой.</summary>
        public static LoadingIndicatorView CreateLoader(string name, Transform parent, float size, Color color)
        {
            RectTransform root = CreateRect(name, parent);
            root.sizeDelta = new Vector2(size, size);

            // Отступы заданы якорями, чтобы индикатор корректно масштабировался под любой размер.
            Image spinner = CreateImage("Spinner", root, color);
            StretchRelative(spinner.rectTransform, 0.15f);

            Image hole = CreateImage("Hole", spinner.rectTransform, new Color(0f, 0f, 0f, 0.35f));
            StretchRelative(hole.rectTransform, 0.26f);

            var loader = root.gameObject.AddComponent<LoadingIndicatorView>();
            SerializedSetter.Set(loader, "_spinner", spinner.rectTransform);
            return loader;
        }
    }
}
