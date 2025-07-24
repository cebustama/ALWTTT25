using UnityEngine;

namespace ALWTTT
{
    [CreateAssetMenu(fileName = " New EnemyCharacterData", 
        menuName = "ALWTTT/Characters/EnemyCharacterData")]
    public class EnemyCharacterData : ScriptableObject
    {
        [SerializeField] private string enemyID;
        [SerializeField] private string enemyName;
        [SerializeField][TextArea] private string enemyDescription;
        [SerializeField] private int maxVibe;
        [SerializeField] private EnemyBase enemyPrefab;
    }
}