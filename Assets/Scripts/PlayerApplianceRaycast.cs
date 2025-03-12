using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class PlayerApplianceRaycast : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]

    public PlayerController player;

    public GameObject selectedAppliance;

    [Header("STATS")]

    public LayerMask layerMask;

    private Color defaultColor;
    [SerializeField] private Color selectedColor;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        } else {
            defaultColor = selectedColor;
        }
    }

    private void Update() {

        if (GameStateManager.currentState == GameState.PLAYING) {

            // Raycast to select counter
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, player.reach, layerMask) 
                && hit.collider.CompareTag("Appliance") && hit.collider.gameObject.TryGetComponent<ApplianceBase>(out var script)) {

                // Assign currently selected appliance
                ServerAssignAppliance(this, hit.collider.gameObject);

                // Temporarily change material color locally
                if (selectedAppliance && selectedAppliance.TryGetComponent<MeshRenderer>(out var appliance)) {

                    // Set default color if not set yet
                    if (defaultColor == selectedColor) {
                        defaultColor = appliance.material.color;
                    }

                    appliance.material.color = selectedColor;
                }

                // Retrieve item back from appliance
                if (Input.GetKeyDown(KeyCode.E) && !player.itemPickup.pickupInHand && script.heldFood) {
                    script.ServerRetrieveItem(this);
                }
                // If the player presses E and is holding an item—
                else if (Input.GetKeyDown(KeyCode.E)) {

                    script.ServerUseAppliance(this);
                }
                
            } else {

                if (selectedAppliance && selectedAppliance.TryGetComponent<ApplianceBase>(out var appliance)) {
                    appliance.ServerLeftAppliance(this);
                }

                // Temporarily revert material color locally
                if (selectedAppliance && defaultColor != selectedColor) {

                    if (selectedAppliance.TryGetComponent<MeshRenderer>(out var mesh)) {
                        mesh.material.color = defaultColor;
                    }

                    // Reset default color
                    defaultColor = selectedColor;
                }

                // Unassign currently selected appliance
                ServerUnassignAppliance(this);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAssignAppliance(PlayerApplianceRaycast playerRaycast, GameObject appliance) {
        AssignAppliance(playerRaycast, appliance);
    }

    [ObserversRpc]
    public void AssignAppliance(PlayerApplianceRaycast playerRaycast, GameObject appliance) {

        if (playerRaycast.selectedAppliance != appliance) {

            playerRaycast.selectedAppliance = appliance;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnassignAppliance(PlayerApplianceRaycast playerRaycast) {
        UnassignAppliance(playerRaycast);
    }

    [ObserversRpc]
    public void UnassignAppliance(PlayerApplianceRaycast playerRaycast) {
        playerRaycast.selectedAppliance = null;
    }
}