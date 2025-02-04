using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/Flavor")]
public class Flavor : ScriptableObject
{

    [Tooltip("The text for this Flavor that shows up on orders.")]
    public string orderText;

    /* [Tooltip("The art sprite for an order with this Flavor.")]
    public Sprite spriteDecor; */
}
