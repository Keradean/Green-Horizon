using UnityEngine;
//=== Andy ===//

namespace Andy.Manager
{
    [CreateAssetMenu(menuName = "Announcement/AnnouncementData", fileName = "AnnouncementData")]
    public class AnnouncementData : ScriptableObject
    {
        public string message;
        public Sprite icon;             // optional
        public float duration = 5f;     // wie lange es angezeigt wird
    }
}