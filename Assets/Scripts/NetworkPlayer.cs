using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private Rigidbody tankRigidbody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private NetworkObject projectilePrefab;

    // Update is called once per frame
    void Update()
    {
        if(IsOwner && IsLocalPlayer)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            tankRigidbody.linearVelocity = transform.forward * verticalInput * moveSpeed;
            transform.Rotate(0, horizontalInput * rotateSpeed * Time.deltaTime, 0);

            if( Input.GetKeyDown(KeyCode.Space) )
            {
                Instantiate(projectilePrefab,transform.position + Vector3.up, transform.rotation).Spawn();
            }
        }
    }
}
