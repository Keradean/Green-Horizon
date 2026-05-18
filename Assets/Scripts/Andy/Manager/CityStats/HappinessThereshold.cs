using UnityEngine;
using UnityEngine.UI;
//=== Andy ===//

[System.Serializable]
public class HappinessThreshold
{
    public Sprite icon;          // Das Smiley Bild
    [Range(0f, 100f)]
    public float minValue;       // Ab welchem Prozentwert dieses Bild gilt
}