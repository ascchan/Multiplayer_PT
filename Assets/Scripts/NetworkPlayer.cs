using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Mono.Cecil;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform weaponTip;

    [SerializeField] private TextMeshPro nicknameDisplay;

    public NetworkVariable<int> healthValue = 
        new NetworkVariable<int>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );
    
    public NetworkVariable<FixedString32Bytes> nickname = 
        new NetworkVariable<FixedString32Bytes>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );
    
    public NetworkVariable<Color> skin = 
        new NetworkVariable<Color>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );

    private UIChatSystem uiChat;
    private UIMultiplayer uiMultiplayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        uiChat = FindAnyObjectByType<UIChatSystem>();
        if(IsOwner && IsLocalPlayer)
        {
            uiMultiplayer = FindAnyObjectByType<UIMultiplayer>(FindObjectsInactive.Include);

            healthValue.Value = 3;
            nickname.Value = uiMultiplayer.GetTypedUsername();
            skin.Value = Random.ColorHSV();

            uiChat.OnMessageSent += DisplayNewTextMessageRpc;
        }

        nicknameDisplay.text = nickname.Value.ToString();

        nickname.OnValueChanged += OnNicknameChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldNickname, FixedString32Bytes newNickname)
    {
        Debug.Log("Player " + oldNickname.ToString() + "changed their nickname to " + newNickname.ToString());
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

    private void LateUpdate()
    {
        nicknameDisplay.transform.rotation = Camera.main.transform.rotation;
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
        if(healthValue.Value <= 0)
        {
            this.NetworkObject.Despawn(false);

            Debug.Log(nickname.Value.ToString() + " just got destroyed!");
        }
    }
}