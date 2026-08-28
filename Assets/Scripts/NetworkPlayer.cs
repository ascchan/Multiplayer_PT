using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Mono.Cecil;
using System.Runtime.InteropServices;
using NUnit.Framework.Constraints;
using System.Data.SqlTypes;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform weaponTip;

    [SerializeField] private TextMeshPro nicknameDisplay;
    [SerializeField] private TextMeshPro healthTankText;

    public NetworkVariable<int> healthValue = 
        new NetworkVariable<int>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );
    
    public NetworkVariable<FixedString32Bytes> nickname = 
        new NetworkVariable<FixedString32Bytes>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );

    public NetworkVariable<FixedString64Bytes> networkHealthTankText =
        new NetworkVariable<FixedString64Bytes>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );

    public NetworkVariable<Color> skin = 
        new NetworkVariable<Color>( readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner );

    private UIChatSystem uiChat;
    private UIMultiplayer uiMultiplayer;

    [SerializeField] private MeshRenderer tankHeadMesh;
    [SerializeField] private MeshRenderer tankBodyMesh;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        uiChat = FindAnyObjectByType<UIChatSystem>();


        if(IsOwner && IsLocalPlayer)
        {
            uiMultiplayer = FindAnyObjectByType<UIMultiplayer>(FindObjectsInactive.Include);

            healthValue.Value = 3;
            
            nickname.Value = uiMultiplayer.GetTypedUsername();

            healthTankText.text = healthValue.Value.ToString();

            skin.Value = uiMultiplayer.GetSelectedColor();

            uiChat.OnMessageSent += DisplayNewTextMessageRpc;
        }

        tankBodyMesh.material.color = skin.Value;
        tankHeadMesh.material.color = skin.Value;

        nicknameDisplay.text = nickname.Value.ToString();
        healthTankText.text = healthValue.Value.ToString();

        nickname.OnValueChanged += OnNicknameChanged;
        networkHealthTankText.OnValueChanged += OnHealthTankTextChanged;
        skin.OnValueChanged += OnTankColorChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        nickname.OnValueChanged -= OnNicknameChanged;
        networkHealthTankText.OnValueChanged -= OnHealthTankTextChanged;
        skin.OnValueChanged -= OnTankColorChanged;
    }

    private void OnTankColorChanged(Color oldColor, Color newColor)
    {
        tankBodyMesh.material.color = newColor;
        tankHeadMesh.material.color = newColor;
    }

    private void OnNicknameChanged(FixedString32Bytes oldNickname, FixedString32Bytes newNickname)
    {
        Debug.Log("Player " + oldNickname.ToString() + " changed their nickname to " + newNickname.ToString());
        nicknameDisplay.text = newNickname.ToString();
    }

    private void OnHealthTankTextChanged(FixedString64Bytes oldHealthTankText, FixedString64Bytes newHealthTankText)
    {
        Debug.Log("Player " + oldHealthTankText.ToString() + " changed their health value to " + newHealthTankText.ToString());
        healthTankText.text = newHealthTankText.ToString();
        UpdateTextServerRpc(newHealthTankText);
        UpdateHealthTextPlayersRpc(newHealthTankText);
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
                ShootProjectileRpc();
            }
            
            healthTankText.text = healthValue.Value.ToString();
            UpdateTextServerRpc(healthValue.Value.ToString());
            UpdateHealthTextPlayersRpc(healthValue.Value.ToString());

        }
    }

    private void LateUpdate()
    {
        nicknameDisplay.transform.rotation = Camera.main.transform.rotation;
        healthTankText.transform.rotation = Camera.main.transform.rotation;
    }

    [Rpc(SendTo.Server)]
    public void ShootProjectileRpc()
    {
        NetworkObject cloneProjectile = 
            Instantiate(projectilePrefab, weaponTip.position, weaponTip.rotation);
        cloneProjectile.Spawn();
    }

    [Rpc(SendTo.Server)]
    private void UpdateTextServerRpc(FixedString64Bytes newHealth)
    {
        healthTankText.text = newHealth.ToString();
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateHealthTextPlayersRpc(FixedString64Bytes newHealth)
    {
        healthTankText.text = newHealth.ToString();
    } 

    [Rpc(SendTo.Server)]
    public void DespawnWithChildrenRpc()
    {
        NetworkObject[] childNetworkObjects = GetComponentsInChildren<NetworkObject>();

        foreach (var netObj in childNetworkObjects)
        {
            if (netObj != null && netObj != NetworkObject && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }

        // Finally despawn the root parent
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
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
        healthTankText.text = healthValue.Value.ToString();

        if (healthValue.Value <= 0)
        {
            Debug.Log(nickname.Value.ToString() + " just got destroyed!");
            DisplayNewTextMessageRpc("The tank " + nickname.Value.ToString() + " just got destroyed!");

            DespawnWithChildrenRpc();
            // this.NetworkObject.Despawn(false);
            // ^ work under Distributed Authority
            // Solution would be to send RPC to server

        }
    }
}   