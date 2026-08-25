using UnityEngine;
using Unity.Netcode;
public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody projectileRigidbody;
    [SerializeField] private float projectileForce;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer)
        {
            projectileRigidbody.AddForce(transform.forward * projectileForce);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {/*
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn();*/
        }
    }

}
