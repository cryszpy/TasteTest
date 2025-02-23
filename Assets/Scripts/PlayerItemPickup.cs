using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class PlayerItemPickup : NetworkBehaviour
{

    public PlayerController player;

    public GameObject itemPoint;

    public GameObject selectedPickup;
    public GameObject pickupInHand;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider collider) {

        if (GameStateManager.currentState != GameState.PLAYING) {
            return;
        }

        if (collider.CompareTag("Pickup")) {

            selectedPickup = collider.gameObject;

            if (collider.TryGetComponent<Pickup>(out var script)) {
                script.spriteRenderer.sprite = script.selectedSprite;
            }

            Debug.Log(player.OwnerId + " in radius!");
        }
    }

    private void OnTriggerExit(Collider collider) {

        if (GameStateManager.currentState != GameState.PLAYING) {
            return;
        }

        if (collider.CompareTag("Pickup")) {

            if (selectedPickup == collider.gameObject) {
                selectedPickup = null;
            }

            if (collider.TryGetComponent<Pickup>(out var script)) {
                script.spriteRenderer.sprite = script.normalSprite;
            }

            Debug.Log(player.OwnerId + " exited radius!");
        }
    }

    private void Update() {

        if (GameStateManager.currentState == GameState.PLAYING) {

            // If the player presses E and is holding an item—
            if (Input.GetKeyDown(KeyCode.E) && selectedPickup && !pickupInHand) {

                if (selectedPickup.TryGetComponent<Pickup>(out var pickup)) {

                    if (!pickup.heldBy) {

                        // Pickup said item
                        ServerPickupItem(selectedPickup, itemPoint, this, gameObject);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Q) && pickupInHand) {

                // Drop held item
                ServerDropItem(pickupInHand);
            }
        }
    }

    // Starts pickup function on server side
    [ServerRpc(RequireOwnership = false)]
    public void ServerPickupItem(GameObject itemObj, GameObject anchor, PlayerItemPickup script, GameObject player) {
        PickupItem(itemObj, anchor, script, player);
    }

    // Starts pickup, all observers know
    [ObserversRpc]
    public void PickupItem(GameObject itemObj, GameObject anchor, PlayerItemPickup script, GameObject player) {

        if (itemObj.TryGetComponent<Pickup>(out var ingredient)) {
            ingredient.heldBy = script;
        }
        
        // Disables gravity while being held
        if (itemObj.TryGetComponent<Rigidbody>(out var rb)) {
            rb.Sleep();
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Prevents player from picking other objects up by disabling collider
        if (player.TryGetComponent<PlayerController>(out var p)) {
            p.pickupRadius.enabled = false;
        }

        // Changes position of the pickup
        itemObj.transform.position = anchor.transform.position;

        // Parents the pickup to the player
        itemObj.transform.parent = player.transform;

        // Sets pickup as currently held object
        script.pickupInHand = itemObj;

        // Enables object if disabled
        if (!script.pickupInHand.activeInHierarchy) {
            script.pickupInHand.SetActive(true);
        }
    }

    // Tells the server to drop an item
    [ServerRpc(RequireOwnership = false)]
    public void ServerDropItem(GameObject obj) {
        DropItem(obj);
    }

    // Drops an item, which is told to all observers
    [ObserversRpc]
    public void DropItem(GameObject obj) {
        
        // Resets parent
        obj.transform.parent = null;

        // Re-enables gravity for the dropped item
        if (obj.TryGetComponent<Rigidbody>(out var rb)) {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Resets currently held item to nothing
        pickupInHand = null;

        // Re-enables player pickup collider
        if (player.TryGetComponent<PlayerController>(out var p)) {
            p.pickupRadius.enabled = true;
        }

        if (obj.TryGetComponent<Pickup>(out var ingredient)) {
            ingredient.heldBy = null;
        }
    }
}
