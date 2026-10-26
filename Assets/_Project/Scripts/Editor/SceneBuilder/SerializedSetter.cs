using UnityEditor;
using UnityEngine;

namespace TestTask.EditorTools.SceneBuilder
{
    /// <summary>
    /// Заполняет приватные [SerializeField] поля компонентов через SerializedObject.
    /// </summary>
    internal static class SerializedSetter
    {
        public static void Set(Object target, string propertyName, Object value)
        {
            Apply(target, propertyName, property => property.objectReferenceValue = value);
        }

        public static void SetInt(Object target, string propertyName, int value)
        {
            Apply(target, propertyName, property => property.intValue = value);
        }

        public static void SetFloat(Object target, string propertyName, float value)
        {
            Apply(target, propertyName, property => property.floatValue = value);
        }

        public static void SetArray(Object target, string propertyName, Object[] values)
        {
            Apply(target, propertyName, property =>
            {
                property.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });
        }

        private static void Apply(Object target, string propertyName, System.Action<SerializedProperty> apply)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogError($"[SceneBuilder] Field '{propertyName}' not found on {target.GetType().Name}", target);
                return;
            }

            apply(property);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
