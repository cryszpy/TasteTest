using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class CameraLookAt : NetworkBehaviour
{

    [Range(1, 80)]
    public float viewRangeBase;

    public GameObject kitchenCenter;
    
    public List<GameObject> players;

    private void Update() {
        if (players.Count > 0) {
            ServerUpdateTransform(gameObject);
        }
    }

    [ObserversRpc]
    public void AddPlayer(CameraLookAt cameraLookAt, GameObject playerToAdd) {
        if (!cameraLookAt.players.Contains(playerToAdd)) {
            cameraLookAt.players.Add(playerToAdd);
            Debug.Log("Added player: " + playerToAdd.GetComponent<PlayerController>().OwnerId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateTransform(GameObject lookAtObj) {

        UpdateTransform(lookAtObj);
    }

    [ObserversRpc]
    private void UpdateTransform(GameObject lookAtObj) {

        if (lookAtObj.TryGetComponent<CameraLookAt>(out var lookAt)) {
            
            if (lookAt.players.Count > 0) {

                float viewRange = Mathf.Clamp(lookAt.viewRangeBase, 1, 80);

                Vector3 playerSum = Vector3.zero;

                foreach (var player in lookAt.players) {
                    playerSum += player.transform.position;
                }

                Vector3 playerTarget = playerSum / lookAt.players.Count;

                var cameraTargetPosition = (lookAt.kitchenCenter.transform.position + (viewRange - 1) * playerTarget) / viewRange;
                lookAtObj.transform.position = cameraTargetPosition;

            } else {
                lookAtObj.transform.position = lookAt.kitchenCenter.transform.position;
            }
        }
    }
}
