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

    public NetworkVariable<int> healthValue;
    private UIChatSystem uiChat;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        uiChat = FindAnyObjectByType<UIChatSystem>();
        if(IsOwner && IsLocalPlayer)
        {
            healthValue.Value = 3;
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
                ShootProjectile();
            }
        }
    }

    public void ShootProjectile()
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

    [Rpc(SendTo.Owner)]
    public void DecreaseHealthRpc()
    {
        healthValue.Value--;
    }
}