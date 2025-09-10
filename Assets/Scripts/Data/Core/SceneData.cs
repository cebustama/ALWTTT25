using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "Scene Data", menuName = "ALWTTT/Settings/Scene")]
    public class SceneData : ScriptableObject
    {
        public int mainMenuSceneIndex = 0;
        public int sectorMapSceneIndex = 1;
        public int shipInteriorSceneIndex = 2;
        public int gigSceneIndex = 3;
    }
}