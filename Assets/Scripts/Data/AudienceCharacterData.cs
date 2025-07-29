using UnityEngine;

namespace ALWTTT.Audience
{
    [CreateAssetMenu(fileName = " New AudienceCharacterData", 
        menuName = "ALWTTT/Characters/AudienceCharacterData")]
    public class AudienceCharacterData : ScriptableObject
    {
        [SerializeField] private string characterID;
        [SerializeField] private string characterName;
        [SerializeField][TextArea] private string characterDescription;
        [SerializeField] private int maxVibe;
        [SerializeField] private AudienceCharacterBase characterPrefab;
    }
}