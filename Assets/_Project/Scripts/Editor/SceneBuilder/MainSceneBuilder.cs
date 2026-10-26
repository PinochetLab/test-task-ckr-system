using System.Collections.Generic;
using System.Linq;
using TestTask.App.Installers;
using TestTask.Core.Installers;
using TestTask.Core.Navigation;
using TestTask.Core.Navigation.View;
using TestTask.Core.Network;
using TestTask.Core.UI;
using TestTask.Features.Clicker.Config;
using TestTask.Features.Clicker.Effects;
using TestTask.Features.Clicker.Installers;
using TestTask.Features.Clicker.View;
using TestTask.Features.Dogs.Config;
using TestTask.Features.Dogs.Installers;
using TestTask.Features.Dogs.View;
using TestTask.Features.Weather.Config;
using TestTask.Features.Weather.Installers;
using TestTask.Features.Weather.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using static TestTask.EditorTools.SceneBuilder.UiFactory;

namespace TestTask.EditorTools.SceneBuilder
{
    /// <summary>
    /// Собирает сцену Main: UI из цветных прямоугольников, префабы, конфиги и SceneContext.
    /// Повторный запуск пересоздаёт UI и префабы; конфиги не перезаписываются.
    /// </summary>
    public static class MainSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string RootFolder = "Assets/_Project";
        private const string ConfigsFolder = RootFolder + "/Configs";
        private const string PrefabsFolder = RootFolder + "/Prefabs";
        private const string UiPrefabsFolder = PrefabsFolder + "/UI";
        private const string EffectsPrefabsFolder = PrefabsFolder + "/Effects";
        private const string AudioFolder = RootFolder + "/Audio";

        private static readonly string[] GeneratedRootNames = { "UI", "EventSystem", "SceneContext", "Audio" };

        private static class Palette
        {
            public static readonly Color Background = new Color(0.11f, 0.11f, 0.13f);
            public static readonly Color Panel = new Color(0.18f, 0.18f, 0.22f);
            public static readonly Color TabBar = new Color(0.15f, 0.15f, 0.18f);
            public static readonly Color ListItem = new Color(0.24f, 0.24f, 0.3f);
            public static readonly Color Text = new Color(0.93f, 0.93f, 0.95f);
            public static readonly Color MutedText = new Color(0.65f, 0.65f, 0.7f);
            public static readonly Color DarkText = new Color(0.12f, 0.12f, 0.15f);
            public static readonly Color Accent = new Color(0.25f, 0.55f, 0.95f);
            public static readonly Color ClickButton = new Color(0.95f, 0.55f, 0.2f);
            public static readonly Color Currency = new Color(1f, 0.82f, 0.2f);
            public static readonly Color EnergyBack = new Color(0.25f, 0.25f, 0.3f);
            public static readonly Color Error = new Color(0.95f, 0.4f, 0.4f);
            public static readonly Color PopupDim = new Color(0f, 0f, 0f, 0.65f);
            public static readonly Color PopupPanel = new Color(0.95f, 0.95f, 0.97f);
        }

        private sealed class Configs
        {
            public ConfigsInstaller Installer;
        }

        private sealed class Prefabs
        {
            public LoadingIndicatorView Loader;
            public ScreenTitleView ScreenTitle;
            public TabButtonView TabButton;
            public FlyingCurrencyView FlyingCurrency;
            public UiParticleView Particle;
            public BreedItemView BreedItem;
        }

        // Префабы текущей сборки: нужны при создании объектов сцены и других префабов.
        private static Prefabs _prefabs;

        /// <summary>
        /// Пересоздаёт UI сцены и префабы с нуля. UI теперь живёт в сцене и редактируется вручную,
        /// поэтому скрипт оставлен как резервный и спрашивает подтверждение.
        /// </summary>
        [MenuItem("Tools/TestTask/Legacy/Rebuild Main Scene (overwrites UI)")]
        public static void Build()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild Main Scene",
                "UI в сцене Main и UI-префабы будут пересозданы с нуля. Ручные правки UI потеряются. Продолжить?",
                "Пересоздать",
                "Отмена");

            if (!confirmed)
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder(RootFolder);
            EnsureFolder(ConfigsFolder);
            EnsureFolder(PrefabsFolder);
            EnsureFolder(AudioFolder);

            // Сначала открываем сцену: OpenScene выгружает неиспользуемые ассеты,
            // и ссылки на созданные до этого префабы становятся невалидными.
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveGeneratedObjects(scene);

            Configs configs = CreateConfigs();
            Prefabs prefabs = CreatePrefabs();
            BuildSceneObjects(configs, prefabs);
            _prefabs = null;

            foreach (GameObject root in scene.GetRootGameObjects())
                RecordPrefabOverrides(root);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AddSceneToBuildSettings();
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            AssetDatabase.SaveAssets();
            Debug.Log("[SceneBuilder] Main scene built. Set the Game view to a portrait resolution (e.g. 1080x1920) and press Play.");
        }

        // ---------- Конфиги ----------

        private static Configs CreateConfigs()
        {
            var network = LoadOrCreate<NetworkConfig>($"{ConfigsFolder}/NetworkConfig.asset");
            var clicker = LoadOrCreate<ClickerConfig>($"{ConfigsFolder}/ClickerConfig.asset");
            var weather = LoadOrCreate<WeatherConfig>($"{ConfigsFolder}/WeatherConfig.asset");
            var dogs = LoadOrCreate<DogsConfig>($"{ConfigsFolder}/DogsConfig.asset");
            var installer = LoadOrCreate<ConfigsInstaller>($"{ConfigsFolder}/ConfigsInstaller.asset");

            if (clicker.ClickSound == null)
            {
                AudioClip clip = ClickSoundGenerator.GetOrCreate($"{AudioFolder}/Click.wav");
                SerializedSetter.Set(clicker, "_clickSound", clip);
            }

            SerializedSetter.Set(installer, "_network", network);
            SerializedSetter.Set(installer, "_clicker", clicker);
            SerializedSetter.Set(installer, "_weather", weather);
            SerializedSetter.Set(installer, "_dogs", dogs);

            return new Configs { Installer = installer };
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ---------- Префабы ----------

        private static Prefabs CreatePrefabs()
        {
            EnsureFolder(UiPrefabsFolder);
            EnsureFolder(EffectsPrefabsFolder);

            // Префабы из прошлой версии сборщика лежали в корне Prefabs: переносим, чтобы сохранить GUID.
            MoveAssetIfExists($"{PrefabsFolder}/BreedItem.prefab", $"{UiPrefabsFolder}/BreedItem.prefab");
            MoveAssetIfExists($"{PrefabsFolder}/FlyingCurrency.prefab", $"{EffectsPrefabsFolder}/FlyingCurrency.prefab");
            MoveAssetIfExists($"{PrefabsFolder}/UiParticle.prefab", $"{EffectsPrefabsFolder}/UiParticle.prefab");

            // Порядок важен: BreedItem содержит вложенный префаб LoadingIndicator.
            _prefabs = new Prefabs();
            _prefabs.Loader = SavePrefab<LoadingIndicatorView>(BuildLoaderPrefab(), $"{UiPrefabsFolder}/LoadingIndicator.prefab");
            _prefabs.ScreenTitle = SavePrefab<ScreenTitleView>(BuildScreenTitlePrefab(), $"{UiPrefabsFolder}/ScreenTitle.prefab");
            _prefabs.TabButton = SavePrefab<TabButtonView>(BuildTabButtonPrefab(), $"{UiPrefabsFolder}/TabButton.prefab");
            _prefabs.BreedItem = SavePrefab<BreedItemView>(BuildBreedItem(), $"{UiPrefabsFolder}/BreedItem.prefab");
            _prefabs.FlyingCurrency = SavePrefab<FlyingCurrencyView>(BuildFlyingCurrency(), $"{EffectsPrefabsFolder}/FlyingCurrency.prefab");
            _prefabs.Particle = SavePrefab<UiParticleView>(BuildParticle(), $"{EffectsPrefabsFolder}/UiParticle.prefab");
            return _prefabs;
        }

        private static GameObject BuildLoaderPrefab()
        {
            LoadingIndicatorView loader = CreateLoader("LoadingIndicator", null, 90f, Palette.Accent);
            return loader.gameObject;
        }

        private static GameObject BuildScreenTitlePrefab()
        {
            Text text = CreateText("ScreenTitle", null, "Title", 64, Palette.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            PlaceTop(text.rectTransform, 40f, new Vector2(1000f, 100f));

            var view = text.gameObject.AddComponent<ScreenTitleView>();
            SerializedSetter.Set(view, "_text", text);
            return text.gameObject;
        }

        private static GameObject BuildTabButtonPrefab()
        {
            Button button = CreateButton("TabButton", null, Palette.Panel, "Tab", 40, Palette.MutedText, out Text label);
            var image = (Image)button.targetGraphic;

            var view = button.gameObject.AddComponent<TabButtonView>();
            SerializedSetter.Set(view, "_button", button);
            SerializedSetter.Set(view, "_background", image);
            SerializedSetter.Set(view, "_label", label);
            return button.gameObject;
        }

        /// <summary>Создаёт экземпляр префаба со связью с ассетом (prefab instance).</summary>
        private static T InstantiatePrefab<T>(T prefab, Transform parent) where T : Component
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
            return instance.GetComponent<T>();
        }

        private static LoadingIndicatorView InstantiateLoader(Transform parent, float size)
        {
            LoadingIndicatorView loader = InstantiatePrefab(_prefabs.Loader, parent);
            var rect = (RectTransform)loader.transform;
            rect.sizeDelta = new Vector2(size, size);
            return loader;
        }

        private static GameObject BuildFlyingCurrency()
        {
            RectTransform root = CreateRect("FlyingCurrency", null);
            root.sizeDelta = new Vector2(44f, 44f);
            AddImage(root, Palette.Currency);

            Image inner = CreateImage("Inner", root, new Color(1f, 0.68f, 0.1f));
            Stretch(inner.rectTransform, 10f, 10f, 10f, 10f);

            root.gameObject.AddComponent<FlyingCurrencyView>();
            return root.gameObject;
        }

        private static GameObject BuildParticle()
        {
            RectTransform root = CreateRect("UiParticle", null);
            root.sizeDelta = new Vector2(18f, 18f);
            Image image = AddImage(root, Color.white);

            var view = root.gameObject.AddComponent<UiParticleView>();
            SerializedSetter.Set(view, "_image", image);
            return root.gameObject;
        }

        private static GameObject BuildBreedItem()
        {
            RectTransform root = CreateRect("BreedItem", null);
            root.sizeDelta = new Vector2(900f, 110f);

            Image background = AddImage(root, Palette.ListItem, true);
            Button button = AddButton(background);

            var layout = root.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 110f;
            layout.preferredHeight = 110f;

            Text label = CreateText("Label", root, "1 - Breed", 40, Palette.Text, TextAnchor.MiddleLeft);
            Stretch(label.rectTransform, 36f, 120f, 0f, 0f);

            LoadingIndicatorView loader = InstantiateLoader(root, 56f);
            loader.name = "Loader";
            Place((RectTransform)loader.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-32f, 0f), new Vector2(56f, 56f));
            loader.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<BreedItemView>();
            SerializedSetter.Set(view, "_button", button);
            SerializedSetter.Set(view, "_label", label);
            SerializedSetter.Set(view, "_loader", loader);
            return root.gameObject;
        }

        private static T SavePrefab<T>(GameObject instance, string path) where T : Component
        {
            RecordPrefabOverrides(instance);
            PrefabUtility.SaveAsPrefabAsset(instance, path, out bool success);
            Object.DestroyImmediate(instance);

            if (!success)
                throw new System.InvalidOperationException($"[SceneBuilder] Failed to save prefab {path}");

            // Берём ассет заново из базы, а не объект, который вернул SaveAsPrefabAsset.
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var component = prefab != null ? prefab.GetComponent<T>() : null;

            if (component == null)
                throw new System.InvalidOperationException($"[SceneBuilder] Prefab {path} has no {typeof(T).Name}");

            return component;
        }

        // ---------- Сцена ----------

        private static void RemoveGeneratedObjects(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                bool generated = GeneratedRootNames.Contains(root.name);
                bool strayEventSystem = root.GetComponent<EventSystem>() != null;

                if (generated || strayEventSystem)
                    Object.DestroyImmediate(root);
            }
        }

        private static void BuildSceneObjects(Configs configs, Prefabs prefabs)
        {
            // Звук
            var audioGo = new GameObject("Audio");
            var audioSource = audioGo.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // EventSystem
            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystemGo.AddComponent<StandaloneInputModule>();
#endif

            // Canvas
            RectTransform canvasRect = CreateRect("UI", null);
            var canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasRect.gameObject.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("Background", canvasRect, Palette.Background);
            Stretch(background.rectTransform);

            RectTransform safeArea = Stretch(CreateRect("SafeArea", canvasRect));
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            const float tabBarHeight = 170f;
            RectTransform screens = Stretch(CreateRect("Screens", safeArea), 0f, 0f, 0f, tabBarHeight);

            ClickerView clickerView = BuildClickerScreen(screens);
            WeatherView weatherView = BuildWeatherScreen(screens);
            DogBreedsView dogsView = BuildDogsScreen(screens, out RectTransform listContent);

            TabBarView tabBar = BuildTabBar(safeArea, tabBarHeight);
            BreedPopupView popup = BuildBreedPopup(safeArea);

            // Слой эффектов внутри экрана кликера: выключается вместе с вкладкой, на других вкладках не виден.
            RectTransform effectsLayer = Stretch(CreateRect("EffectsLayer", clickerView.transform));

            // SceneContext + инсталлеры
            var contextGo = new GameObject("SceneContext");
            var context = contextGo.AddComponent<SceneContext>();

            var coreInstaller = contextGo.AddComponent<CoreInstaller>();
            SerializedSetter.Set(coreInstaller, "_tabBar", tabBar);
            SerializedSetter.Set(coreInstaller, "_audioSource", audioSource);

            var clickerInstaller = contextGo.AddComponent<ClickerInstaller>();
            SerializedSetter.Set(clickerInstaller, "_view", clickerView);
            SerializedSetter.Set(clickerInstaller, "_effectsLayer", effectsLayer);
            SerializedSetter.Set(clickerInstaller, "_flyingCurrencyPrefab", prefabs.FlyingCurrency);
            SerializedSetter.Set(clickerInstaller, "_particlePrefab", prefabs.Particle);

            var weatherInstaller = contextGo.AddComponent<WeatherInstaller>();
            SerializedSetter.Set(weatherInstaller, "_view", weatherView);

            var dogsInstaller = contextGo.AddComponent<DogsInstaller>();
            SerializedSetter.Set(dogsInstaller, "_view", dogsView);
            SerializedSetter.Set(dogsInstaller, "_popup", popup);
            SerializedSetter.Set(dogsInstaller, "_listContent", listContent);
            SerializedSetter.Set(dogsInstaller, "_itemPrefab", prefabs.BreedItem);

            ValidateReferences(coreInstaller, clickerInstaller, weatherInstaller, dogsInstaller);

            context.Installers = new List<MonoInstaller> { coreInstaller, clickerInstaller, weatherInstaller, dogsInstaller };
            context.ScriptableObjectInstallers = new List<ScriptableObjectInstaller> { configs.Installer };
            EditorUtility.SetDirty(context);
        }

        private static RectTransform CreateScreen<T>(string name, Transform parent, out T view) where T : TabScreenView
        {
            RectTransform rect = Stretch(CreateRect(name, parent));
            rect.gameObject.AddComponent<CanvasGroup>();
            view = rect.gameObject.AddComponent<T>();
            return rect;
        }

        private static void CreateTitle(Transform screen, string title)
        {
            ScreenTitleView view = InstantiatePrefab(_prefabs.ScreenTitle, screen);
            view.name = "Title";
            view.Title = title;
        }

        private static ClickerView BuildClickerScreen(Transform parent)
        {
            RectTransform screen = CreateScreen("ClickerScreen", parent, out ClickerView view);
            CreateTitle(screen, "Кликер");

            // Валюта
            Image currencyIcon = CreateImage("CurrencyIcon", screen, Palette.Currency);
            PlaceTop(currencyIcon.rectTransform, 200f, new Vector2(64f, 64f));
            currencyIcon.rectTransform.anchoredPosition = new Vector2(-260f, -200f);

            Text currencyText = CreateText("CurrencyText", screen, "Валюта: 0", 56, Palette.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            Place(currencyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(-200f, -182f), new Vector2(700f, 100f));

            // Энергия
            Image energyBack = CreateImage("EnergyBar", screen, Palette.EnergyBack);
            PlaceTop(energyBack.rectTransform, 320f, new Vector2(900f, 70f));

            Image energyFill = CreateImage("Fill", energyBack.rectTransform, Palette.Accent);
            RectTransform fillRect = energyFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Text energyText = CreateText("EnergyText", energyBack.rectTransform, "Энергия: 0 / 0", 36, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(energyText.rectTransform);

            // Кнопка
            Button clickButton = CreateButton("ClickButton", screen, Palette.ClickButton, "КЛИК", 96, Color.white, out _);
            PlaceCenter((RectTransform)clickButton.transform, new Vector2(0f, -60f), new Vector2(520f, 520f));
            var animator = clickButton.gameObject.AddComponent<ButtonPunchAnimator>();

            SerializedSetter.Set(view, "_clickButton", clickButton);
            SerializedSetter.Set(view, "_buttonAnimator", animator);
            SerializedSetter.Set(view, "_currencyText", currencyText);
            SerializedSetter.Set(view, "_currencyIcon", currencyIcon.rectTransform);
            SerializedSetter.Set(view, "_energyText", energyText);
            SerializedSetter.Set(view, "_energyFill", fillRect);
            SerializedSetter.Set(view, "_energyFillImage", energyFill);
            return view;
        }

        private static WeatherView BuildWeatherScreen(Transform parent)
        {
            RectTransform screen = CreateScreen("WeatherScreen", parent, out WeatherView view);
            CreateTitle(screen, "Погода");

            Image card = CreateImage("Card", screen, Palette.Panel);
            PlaceCenter(card.rectTransform, new Vector2(0f, 100f), new Vector2(880f, 360f));

            Image icon = CreateImage("Icon", card.rectTransform, Color.white);
            icon.preserveAspect = true;
            Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(200f, 200f));

            Text forecast = CreateText("ForecastText", card.rectTransform, string.Empty, 64, Palette.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(forecast.rectTransform, 290f, 30f, 20f, 20f);

            LoadingIndicatorView loader = InstantiateLoader(card.rectTransform, 90f);
            loader.name = "Loader";
            PlaceCenter((RectTransform)loader.transform, Vector2.zero, new Vector2(90f, 90f));

            Text error = CreateText("ErrorText", screen, string.Empty, 36, Palette.Error);
            PlaceCenter(error.rectTransform, new Vector2(0f, -170f), new Vector2(900f, 100f));

            SerializedSetter.Set(view, "_icon", icon);
            SerializedSetter.Set(view, "_forecastText", forecast);
            SerializedSetter.Set(view, "_errorText", error);
            SerializedSetter.Set(view, "_loader", loader);
            return view;
        }

        private static DogBreedsView BuildDogsScreen(Transform parent, out RectTransform content)
        {
            RectTransform screen = CreateScreen("DogsScreen", parent, out DogBreedsView view);
            CreateTitle(screen, "Породы собак");

            // Прокручиваемый список
            RectTransform viewport = Stretch(CreateRect("List", screen), 40f, 40f, 170f, 30f);
            viewport.gameObject.AddComponent<RectMask2D>();
            Image hitArea = AddImage(viewport, new Color(0f, 0f, 0f, 0f), true);

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            hitArea.raycastTarget = true;

            LoadingIndicatorView loader = InstantiateLoader(screen, 100f);
            loader.name = "Loader";
            PlaceCenter((RectTransform)loader.transform, Vector2.zero, new Vector2(100f, 100f));

            Text error = CreateText("ErrorText", screen, string.Empty, 36, Palette.Error);
            PlaceCenter(error.rectTransform, new Vector2(0f, 0f), new Vector2(900f, 120f));

            SerializedSetter.Set(view, "_content", content);
            SerializedSetter.Set(view, "_loader", loader);
            SerializedSetter.Set(view, "_errorText", error);
            return view;
        }

        private static TabBarView BuildTabBar(Transform parent, float height)
        {
            Image bar = CreateImage("TabBar", parent, Palette.TabBar, true);
            RectTransform rect = bar.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);

            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var view = bar.gameObject.AddComponent<TabBarView>();

            var buttons = new[]
            {
                BuildTabButton(rect, TabId.Clicker, "Кликер"),
                BuildTabButton(rect, TabId.Weather, "Погода"),
                BuildTabButton(rect, TabId.Dogs, "Собаки")
            };

            SerializedSetter.SetArray(view, "_buttons", buttons.Cast<Object>().ToArray());
            return view;
        }

        private static TabButtonView BuildTabButton(Transform parent, TabId id, string title)
        {
            TabButtonView view = InstantiatePrefab(_prefabs.TabButton, parent);
            view.name = $"Tab_{id}";
            view.GetComponentInChildren<Text>().text = title;
            SerializedSetter.SetInt(view, "_id", (int)id);
            return view;
        }

        private static BreedPopupView BuildBreedPopup(Transform parent)
        {
            RectTransform root = Stretch(CreateRect("BreedPopup", parent));
            var canvasGroup = root.gameObject.AddComponent<CanvasGroup>();

            Image dim = CreateImage("Dim", root, Palette.PopupDim, true);
            Stretch(dim.rectTransform);
            Button dimButton = AddButton(dim, false);

            Image panel = CreateImage("Panel", root, Palette.PopupPanel, true);
            PlaceCenter(panel.rectTransform, Vector2.zero, new Vector2(900f, 1100f));

            Text title = CreateText("Title", panel.rectTransform, "Breed", 56, Palette.DarkText, TextAnchor.MiddleCenter, FontStyle.Bold);
            PlaceTop(title.rectTransform, 40f, new Vector2(820f, 110f));

            // Высоту панели под текст подгоняет BreedPopupView; длинный текст прокручивается во вьюпорте.
            RectTransform viewport = Stretch(CreateRect("DescriptionViewport", panel.rectTransform), 50f, 50f, 180f, 200f);
            viewport.gameObject.AddComponent<RectMask2D>();
            AddImage(viewport, new Color(0f, 0f, 0f, 0f), true);

            Text description = CreateText("Description", viewport, string.Empty, 38, Palette.DarkText, TextAnchor.UpperLeft);
            RectTransform descriptionRect = description.rectTransform;
            descriptionRect.anchorMin = new Vector2(0f, 1f);
            descriptionRect.anchorMax = new Vector2(1f, 1f);
            descriptionRect.pivot = new Vector2(0.5f, 1f);
            descriptionRect.anchoredPosition = Vector2.zero;
            descriptionRect.sizeDelta = Vector2.zero;
            description.verticalOverflow = VerticalWrapMode.Overflow;

            var descriptionScroll = viewport.gameObject.AddComponent<ScrollRect>();
            descriptionScroll.content = descriptionRect;
            descriptionScroll.viewport = viewport;
            descriptionScroll.horizontal = false;
            descriptionScroll.vertical = true;
            descriptionScroll.movementType = ScrollRect.MovementType.Elastic;

            Button close = CreateButton("CloseButton", panel.rectTransform, Palette.Accent, "Закрыть", 44, Color.white, out _);
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(420f, 110f));

            var view = root.gameObject.AddComponent<BreedPopupView>();
            SerializedSetter.Set(view, "_canvasGroup", canvasGroup);
            SerializedSetter.Set(view, "_panel", panel.rectTransform);
            SerializedSetter.Set(view, "_title", title);
            SerializedSetter.Set(view, "_description", description);
            SerializedSetter.Set(view, "_descriptionScroll", descriptionScroll);
            SerializedSetter.Set(view, "_closeButton", close);
            SerializedSetter.Set(view, "_backgroundButton", dimButton);
            return view;
        }

        // ---------- Прочее ----------

        /// <summary>
        /// Фиксирует изменения, сделанные из кода у экземпляров префабов, как overrides.
        /// Без этого Unity может потерять их при сохранении.
        /// </summary>
        private static void RecordPrefabOverrides(GameObject root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(child.gameObject))
                    continue;

                PrefabUtility.RecordPrefabInstancePropertyModifications(child.gameObject);
                foreach (Component component in child.GetComponents<Component>())
                {
                    if (component != null)
                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
            }
        }

        private static void MoveAssetIfExists(string from, string to)
        {
            if (AssetDatabase.LoadMainAssetAtPath(from) == null || AssetDatabase.LoadMainAssetAtPath(to) != null)
                return;

            string error = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"[SceneBuilder] Can't move {from} -> {to}: {error}");
        }

        /// <summary>Проверяет, что все ссылки в инсталлерах заполнены, иначе пишет ошибку в консоль.</summary>
        private static void ValidateReferences(params Object[] targets)
        {
            foreach (Object target in targets)
            {
                var serializedObject = new SerializedObject(target);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.name != "m_Script"
                        && property.objectReferenceValue == null)
                    {
                        Debug.LogError($"[SceneBuilder] {target.GetType().Name}.{property.name} is not assigned", target);
                    }
                }
            }
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == ScenePath))
                return;

            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string name = path.Substring(slash + 1);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
