using System.IO;
using TestTask.Features.Clicker.Effects;
using TestTask.Features.Clicker.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TestTask.EditorTools.SceneBuilder
{
    /// <summary>
    /// Собирает кликабельный объект кликера на сцене (не в UI): спрайт-квадрат с коллайдером
    /// и настоящую ParticleSystem разлёта. UI рисуется поверх без фона, фон даёт камера.
    /// </summary>
    internal static class WorldClickerBuilder
    {
        public const string RootName = "ClickTarget";

        private const string ArtFolder = "Assets/_Project/Art";
        private const string SquareSpritePath = ArtFolder + "/Square.png";
        private const int SquareTextureSize = 64;

        // Прежние UI-эффекты (квадратики на DOTween и «монеты» к счётчику) заменены ParticleSystem.
        private static readonly string[] ObsoleteAssetPaths =
        {
            "Assets/_Project/Prefabs/Effects/UiParticle.prefab",
            "Assets/_Project/Scripts/Runtime/Features/Clicker/Effects/UiParticleView.cs",
            "Assets/_Project/Prefabs/Effects/FlyingCurrency.prefab",
            "Assets/_Project/Scripts/Runtime/Features/Clicker/Effects/FlyingCurrencyView.cs"
        };

        // Старая UI-кнопка была 520 px при эталонной высоте 1920 px; камера ортографическая, size 5 → 10 юнитов по высоте.
        private const float BodySize = 2.7f;
        private const float ShakeStrength = 0.15f;
        private static readonly Vector3 Position = new Vector3(0f, 0.15f, 0f);

        public static readonly Color BackgroundColor = new Color(0.11f, 0.11f, 0.13f, 1f);
        private static readonly Color BodyColor = new Color(0.95f, 0.55f, 0.2f);

        private static readonly string[] ParticleMaterialPaths =
        {
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat",
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat"
        };

        /// <summary>
        /// Переводит открытую сцену на 2D-кликер: убирает UI-кнопку и UI-фон, ставит объект на сцену,
        /// настраивает камеру и удаляет больше не нужные UI-частицы. Можно запускать повторно.
        /// </summary>
        [MenuItem("Tools/TestTask/Setup World Clicker")]
        public static void SetupInOpenScene()
        {
            var view = Object.FindFirstObjectByType<ClickerView>(FindObjectsInactive.Include);
            if (view == null)
            {
                Debug.LogError("[WorldClicker] ClickerView not found in the open scene.");
                return;
            }

            Scene scene = view.gameObject.scene;

            Canvas canvas = view.GetComponentInParent<Canvas>(true);
            if (canvas != null)
                DestroyChild(canvas.rootCanvas.transform, "Background");

            DestroyChild(view.transform, "ClickButton");
            DestroyChild(view.transform, "EffectsLayer");

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == RootName)
                    Object.DestroyImmediate(root);
            }

            Build(view);
            DeleteObsoleteAssets();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[WorldClicker] Clicker object is on the scene now. UI background removed, camera background set.");
        }

        /// <summary>Создаёт объект в корне активной сцены и прописывает ссылки в <paramref name="view"/>.</summary>
        public static ClickTargetView Build(ClickerView view)
        {
            SetupCamera();

            var root = new GameObject(RootName);
            root.transform.position = Position;

            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * BodySize;

            // Анимируется только Body, чтобы коллайдер не «прыгал» вместе с пульсом и тряской.
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = Vector3.one * BodySize;

            var spriteRenderer = body.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetOrCreateSquareSprite();
            spriteRenderer.color = BodyColor;
            spriteRenderer.sortingOrder = 0;

            var animator = body.AddComponent<ButtonPunchAnimator>();
            SerializedSetter.SetFloat(animator, "_shakeStrength", ShakeStrength);

            ParticleSystem particles = CreateClickParticles(root.transform);

            var target = root.AddComponent<ClickTargetView>();
            SerializedSetter.Set(target, "_animator", animator);
            SerializedSetter.Set(target, "_burst", particles);

            SerializedSetter.Set(view, "_clickTarget", target);
            return target;
        }

        private static void SetupCamera()
        {
            Camera camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                Debug.LogError("[WorldClicker] No camera in the scene.");
                return;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;

            // Без рейкастера EventSystem не видит коллайдеры на сцене.
            if (camera.GetComponent<Physics2DRaycaster>() == null)
                camera.gameObject.AddComponent<Physics2DRaycaster>();

            EditorUtility.SetDirty(camera);
        }

        /// <summary>Частицы просто разлетаются во все стороны от центра объекта.</summary>
        private static ParticleSystem CreateClickParticles(Transform parent)
        {
            var go = new GameObject("ClickParticles");
            go.transform.SetParent(parent, false);

            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.38f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(CreateStartColors()) { mode = ParticleSystemGradientMode.RandomColor };
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;
            main.maxParticles = 500;

            // Частицы выпускаются из кода через Emit(count) на каждое нажатие.
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.6f;
            shape.radiusThickness = 1f;
            shape.arc = 360f;

            // Торможение: быстрый вылет и мягкая остановка.
            ParticleSystem.LimitVelocityOverLifetimeModule limit = particles.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 100f;
            limit.dampen = 0f;
            limit.drag = 3f;
            limit.multiplyDragByParticleSize = false;
            limit.multiplyDragByParticleVelocity = false;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.4f), new GradientAlphaKey(0f, 1f) });
            color.color = fade;

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            ParticleSystem.RotationOverLifetimeModule rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-7f, 7f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = LoadParticleMaterial();
            renderer.sortingOrder = 1;

            return particles;
        }

        /// <summary>Три фиксированных цвета, заметных на оранжевом квадрате и тёмном фоне.</summary>
        private static Gradient CreateStartColors()
        {
            var gradient = new Gradient { mode = GradientMode.Fixed };
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.82f, 0.2f), 0.34f),
                    new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0.67f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        private static Material LoadParticleMaterial()
        {
            foreach (string path in ParticleMaterialPaths)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null)
                    return material;
            }

            var fallback = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            if (fallback == null)
                Debug.LogWarning("[WorldClicker] Particle material not found, assign it on ClickParticles manually.");

            return fallback;
        }

        private static Sprite GetOrCreateSquareSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            if (existing != null)
                return existing;

            EnsureFolder(ArtFolder);

            var texture = new Texture2D(SquareTextureSize, SquareTextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[SquareTextureSize * SquareTextureSize];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, 255);

            texture.SetPixels32(pixels);
            File.WriteAllBytes(SquareSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(SquareSpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SquareSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = SquareTextureSize; // 1 юнит
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
        }

        private static void DeleteObsoleteAssets()
        {
            foreach (string path in ObsoleteAssetPaths)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) != null && AssetDatabase.DeleteAsset(path))
                    Debug.Log($"[WorldClicker] Deleted obsolete {path}");
            }
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
