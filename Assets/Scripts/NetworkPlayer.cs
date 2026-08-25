using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform weaponTip;

    private UIChatSystem uiChat;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        uiChat = FindAnyObjectByType<UIChatSystem>();
        if(IsOwner && IsLocalPlayer)
        {
            uiChat.OnMessageSent += DisplayNewTextMessageRpc;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && IsLocalPlayer)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            tankRigidbody.linearVelocity = transform.forward * verticalInput * moveSpeed;
            transform.Rotate(0, horizontalInput * rotateSpeed * Time.deltaTime, 0);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShootProjectileRPC();
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void ShootProjectileRPC()
    {
        NetworkObject cloneProjectile = 
            Instantiate(projectilePrefab, weaponTip.position, weaponTip.rotation);
        cloneProjectile.Spawn();
    }

    [Rpc(SendTo.Everyone)]
    public void DisplayNewTextMessageRpc(FixedString128Bytes messageReceived)
    {
        Debug.Log(messageReceived);
        uiChat.DisplayMessageOnBox(messageReceived);
    }
}