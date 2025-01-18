using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]
 
    //[SerializeField] private float cameraYOffset = 0.4f;
    private CameraLookAt camLookAt;

    public PlayerControls playerControls;
    private InputAction inputMove;
    private InputAction inputPickup;

    [SerializeField] private Rigidbody rb;

    [Header("STATS")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float lookSpeed = 2.0f;
 
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
            camLookAt = GameObject.FindGameObjectWithTag("CamLookAt").GetComponent<CameraLookAt>();
            //cam.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            //cam.Follow = transform;
            ServerAddPlayer(camLookAt, gameObject);
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAddPlayer(CameraLookAt cameraLookAt, GameObject playerToAdd) {
        cameraLookAt.AddPlayer(cameraLookAt, playerToAdd);
    }
 
    void Start()
    {
        //characterController = GetComponent<CharacterController>();
 
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        moveDirection = inputMove.ReadValue<Vector2>();

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = isRunning ? runningSpeed : walkingSpeed;

        moveDirection = new Vector3(moveDirection.x * speed, 0, moveDirection.y * speed);

        RotateInDirection(moveDirection);
    }
 
    private void FixedUpdate()
    {
        ServerMovePlayer(this);
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