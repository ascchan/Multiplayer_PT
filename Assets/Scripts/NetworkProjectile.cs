using UnityEngine;
using Unity.Netcode;
public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody projectileRigidbody;
    [SerializeField] private float projectileForce;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(HasAuthority)
        {
            projectileRigidbody.AddForce(transform.forward * projectileForce);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HasAuthority)
        {
            if( collision.collider.CompareTag("Player") )
            {
                collision.collider.GetComponent<NetworkPlayer>().DecreaseHealthRpc();
            }

            this.NetworkObject.Despawn();

        }
    }

}
