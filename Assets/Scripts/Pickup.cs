using FishNet.Object;
using UnityEngine;

public class Pickup : NetworkBehaviour
{

    public SpriteRenderer spriteRenderer;

    public PlayerItemPickup heldBy;

    public Sprite normalSprite;
    public Sprite selectedSprite;
}