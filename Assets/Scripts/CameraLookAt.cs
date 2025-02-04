using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class CameraLookAt : NetworkBehaviour
{

    [Range(1, 80)]
    public float viewRangeBase;

    public GameObject kitchenCenter;

    private GameStateManager manager;

    private void Start() {
        manager = GameObject.FindGameObjectWithTag("GameStateManager").GetComponent<GameStateManager>();
    }
    
    private void Update() {
        if (manager.players.Count > 0) {
            ServerUpdateTransform(gameObject, manager);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateTransform(GameObject lookAtObj, GameStateManager manager) {

        UpdateTransform(lookAtObj, manager);
    }

    [ObserversRpc]
    private void UpdateTransform(GameObject lookAtObj, GameStateManager manager) {

        if (lookAtObj.TryGetComponent<CameraLookAt>(out var lookAt)) {
            
            if (manager.players.Count > 0) {

                // Reference list of null (disconnected) players
                List<GameObject> nullPlayers = new();

                float viewRange = Mathf.Clamp(lookAt.viewRangeBase, 1, 80);

                Vector3 playerSum = Vector3.zero;

                foreach (var player in manager.players) {

                    // If the player is still active, add this to the sum
                    if (player != null) {
                        playerSum += player.transform.position;
                    } 
                    // If player has disconnected, add it to the reference list
                    else {
                        nullPlayers.Add(player);
                    }
                }

                Vector3 playerTarget = playerSum / manager.players.Count;

                var cameraTargetPosition = (lookAt.kitchenCenter.transform.position + (viewRange - 1) * playerTarget) / viewRange;
                lookAtObj.transform.position = cameraTargetPosition;

                // For each disconnected player, remove the player from the active players list
                foreach (var nullPlayer in nullPlayers) {
                    manager.players.Remove(nullPlayer);
                }

                // Reset null players list
                nullPlayers.Clear();

            } else {
                lookAtObj.transform.position = lookAt.kitchenCenter.transform.position;
            }
        }
    }
}
