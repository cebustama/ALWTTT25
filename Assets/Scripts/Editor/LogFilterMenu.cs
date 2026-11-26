using ALWTTT.DevLogs;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>[S10] Creates (once) and selects the Console filter asset.</summary>
    public static class LogFilterMenu
    {
        private const string AssetPath = "Assets/Resources/" + LogFilterSettingsSO.ResourcePath + ".asset";

        [MenuItem("ALWTTT/Logs ▸ Crear o abrir el filtro de logs")]
        public static void CreateOrOpen()
        {
            var s = AssetDatabase.LoadAssetAtPath<LogFilterSettingsSO>(AssetPath);
            if (s == null)
            {
                s = ScriptableObject.CreateInstance<LogFilterSettingsSO>();
                s.categories = LogFilterSettingsSO.Defaults();
                AssetDatabase.CreateAsset(s, AssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[S10] Filtro de logs creado en {AssetPath}");
            }
            Selection.activeObject = s;
            EditorGUIUtility.PingObject(s);
        }
    }
}
