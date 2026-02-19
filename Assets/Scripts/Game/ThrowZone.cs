using Characters;
using Unity.Netcode;
using UnityEngine;
using Utilities;

/// <summary>
/// Thrown by the human, simulates arc trajectory. Once it settles,
/// spawns a ChickenTrapLander above the landing point to drop the cage down.
/// Server-authoritative — clients just see the visual child object move.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrowZone : NetworkBehaviour
{
    [SerializeField] private ChickenTrapLander trapPrefab;
    [SerializeField] private ParticleSystem onLandParticle;

    private const float MinSpeed = 0.1f;
    private const float SpawnRadiusCheck = 0.4f;
    private const float SpawnHeight = 15;

    private Rigidbody _rb;
    private ITrappable _caught;
    private ulong _caughtNetworkId;
    private float _lifeTime;

    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();

        // Clients don't need to simulate anything — physics is server-driven
        if (!IsServer)
        {
            _rb.isKinematic = true;
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        _lifeTime += Time.fixedDeltaTime;
        float speed = _rb.linearVelocity.sqrMagnitude;

        if (_lifeTime > 0.5f && speed < MinSpeed)
        {
            Vector3 startPoint;
            Vector3 velocity = Vector3.down;

            if (Physics.SphereCast(transform.position, SpawnRadiusCheck, Vector3.up, out RaycastHit hit, SpawnHeight))
            {
                startPoint = transform.position + Vector3.up * (hit.distance - SpawnRadiusCheck * 2);
                velocity *= hit.distance;
            }
            else
            {
                startPoint = transform.position + SpawnHeight * Vector3.up;
                velocity *= SpawnHeight;
            }

            // Detach the visual child before we despawn, so it doesn't vanish with us
            // ChickenTrapLander will re-parent it to the cage when it lands
            Transform visual = transform.GetChild(0);
            visual.SetParent(null, true);

            // Spawn the cage dropper
            ChickenTrapLander trap = Instantiate(trapPrefab, startPoint, Quaternion.identity);
            trap.GetComponent<NetworkObject>().Spawn();
            trap.Initialize(velocity, _caught, _caughtNetworkId);

            PlayLandEffectClientRpc(transform.position);

            NetworkObject.Despawn();
        }
    }

    /// <summary>
    /// Called server-side by CaptureZone after spawning this object.
    /// </summary>
    public void Initialize(Vector3 force, ITrappable caught, ulong caughtNetworkId)
    {
        _rb = GetComponent<Rigidbody>();
        _caught = caught;
        _caughtNetworkId = caughtNetworkId;

        // Move the caught entity to follow the throw zone in world space
        Transform tr = caught.GetTransform();
        tr.SetParent(transform, true);
        tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _rb.linearVelocity = force;
    }

    [ClientRpc]
    private void PlayLandEffectClientRpc(Vector3 position)
    {
        ParticleSystem ps = Instantiate(onLandParticle, position, Quaternion.LookRotation(transform.up));
        ps.Play();
        Destroy(ps.gameObject, 3);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        GizmosExtras.DrawWireSphereCast(transform.position, Vector3.up, SpawnHeight, SpawnRadiusCheck);
    }
}