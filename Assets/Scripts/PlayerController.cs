using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System.Linq;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]
 
    //[SerializeField] private float cameraYOffset = 0.4f;
    private GameStateManager manager;

    public PlayerControls playerControls;
    private InputAction inputMove;
    private InputAction inputPickup;

    [SerializeField] private Rigidbody rb;

    [Header("STATS")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float lookSpeed = 4.0f;
 
    Vector3 moveDirection = Vector3.zero;

    private bool isRunning = false;

    private void OnEnable() {
        inputMove = playerControls.PlayerMovement.Move;
        inputMove.Enable();
    }

    private void OnDisable() {
        inputMove.Disable();
    }

    private void Awake() {
        playerControls = new PlayerControls();
    }
 
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {            
            manager = GameObject.FindGameObjectWithTag("GameStateManager").GetComponent<GameStateManager>();
            //cam.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            //cam.Follow = transform;
            ServerAddPlayer(manager, GameObject.FindGameObjectsWithTag("Player").ToList());

        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAddPlayer(GameStateManager manager, List<GameObject> scannedPlayers) {
        
        foreach (var player in scannedPlayers) {
            AddPlayer(manager, player);
        }
    }

    [ObserversRpc]
    public void AddPlayer(GameStateManager manager, GameObject playerToAdd) {
        if (!manager.players.Contains(playerToAdd)) {
            manager.players.Add(playerToAdd);
            Debug.Log("Added player: " + playerToAdd.GetComponent<PlayerController>().OwnerId);
        }
    }

    private void Update() {

        if (GameStateManager.currentState == GameState.PLAYING) {
        
            moveDirection = inputMove.ReadValue<Vector2>();

            // Press Left Shift to run
            isRunning = Input.GetKey(KeyCode.LeftShift);

            float speed = isRunning ? runningSpeed : walkingSpeed;

            moveDirection = new Vector3(moveDirection.x * speed, 0, moveDirection.y * speed);

            RotateInDirection(moveDirection);
        }
    }
 
    private void FixedUpdate()
    {
        if (GameStateManager.currentState == GameState.PLAYING && base.IsClientInitialized) {
            ServerMovePlayer(this);
        }
    }

    [ServerRpc]
    public void ServerMovePlayer(PlayerController player){
        MovePlayer(player);
    }

    [ObserversRpc]
    public void MovePlayer(PlayerController player) {
        player.rb.linearVelocity = player.moveDirection;
    }

    public void RotateInDirection(Vector3 direction) {

        if (direction.magnitude == 0) return;

        var rotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, lookSpeed);
    }
}