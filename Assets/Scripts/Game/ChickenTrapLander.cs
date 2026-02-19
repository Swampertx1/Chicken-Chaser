using System.Collections;
using Characters;
using Game;
using Unity.Netcode;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(Rigidbody))]
public class ChickenTrapLander : NetworkBehaviour
{
    private const float SpawnSpeed = 1.2f;
    private Rigidbody _rb;
    private ITrappable _caught;

    [SerializeField] private Transform startPoint;
    [SerializeField] private float distance;
    [SerializeField] private ParticleSystem onLandParticle;

    private static readonly Vector3 offset = new Vector3(0, 0.4f, 0);

    void FixedUpdate()
    {
        // Only the server should drive landing logic
        if (!IsServer) return;

        if (Physics.Raycast(startPoint.position, Vector3.down, out RaycastHit hit, distance,
                StaticUtilities.GroundLayers))
        {
            transform.position = hit.point + offset;

            Transform cage = transform.GetChild(0);
            cage.SetParent(null, true);
            cage.GetComponentInChildren<ChickenTrap>().AttachChicken(_caught);

            // Tell all clients to play the particle effect at this position
            PlayLandParticleClientRpc(hit.point);

            NetworkObject.Despawn();
        }
    }

    [ClientRpc]
    private void PlayLandParticleClientRpc(Vector3 hitPoint)
    {
        ParticleSystem ps = Instantiate(onLandParticle, hitPoint, Quaternion.LookRotation(transform.up));
        ps.Play();
        Destroy(ps.gameObject, 3);
    }

    /// <summary>
    /// Called server-side after spawning. Pass the caught entity's NetworkObjectId
    /// so we can resolve the ITrappable reference reliably.
    /// </summary>
    public void Initialize(Vector3 velocity, ITrappable caught, ulong caughtNetworkId)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = velocity * SpawnSpeed;
        _caught = caught;
        StartCoroutine(Emergency());
    }

    private IEnumerator Emergency()
    {
        yield return new WaitForSeconds(3);
        if (!IsSpawned) yield break;
        _caught.OnFreedFromCage();
        NetworkObject.Despawn();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(startPoint.position, Vector3.down * distance);
    }
}