using System.Collections;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TowerGun : MonoBehaviour
{
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private ParticleSystem ShootingSystem2;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private Transform BulletSpawnPoint2;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private float BulletSpeed = 200;
    [SerializeField]
    private float range = 1;

    private Animator Animator;
    public float LastShootTime;
    public bool leftTurn;
    public Transform currentTarget;
    public float currentDistance;
    public int damage = 10;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }
    private void Update()
    {
        LastShootTime -= Time.deltaTime;
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, Mask);
        
        if (colliders.Length > 0)
        {
            foreach (var collider in colliders) 
            {
                if(currentTarget == null)
                {
                    currentTarget = collider.attachedRigidbody.GetComponent<CarAI>().carFront;
                    currentDistance = Vector3.Distance(transform.position, currentTarget.position);
                }
                if(Vector3.Distance(transform.position, collider.transform.position) < currentDistance)
                {
                    currentTarget = collider.attachedRigidbody.GetComponent<CarAI>().carFront;
                    currentDistance = Vector3.Distance(transform.position, currentTarget.position);
                }
            }

        }

            

        if(currentTarget != null )
        {
            currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            if(currentDistance < range)
            {
                Shoot();
                var relativePos = currentTarget.position - transform.position;
                var rotation = Quaternion.LookRotation(relativePos);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 0.03f);
            }
            if (currentTarget.gameObject.layer != (int)Mathf.Log(Mask.value, 2))
                currentTarget = null;

        }
        
    }
    public void Shoot()
    {
        if (LastShootTime < 0)
        {
            
            Vector3 shootpos;
            if(leftTurn)
            {
                leftTurn = !leftTurn;
                shootpos = BulletSpawnPoint.position;
                ShootingSystem.Play();
            }
            else
            {
                leftTurn = !leftTurn;
                shootpos = BulletSpawnPoint2.position;
                ShootingSystem2.Play();
            }
            // Use an object pool instead for these! To keep this tutorial focused, we'll skip implementing one.
            // For more details you can see: https://youtu.be/fsDE_mO4RZM or if using Unity 2021+: https://youtu.be/zyzqA_CPz2E
            //Animator.SetBool("IsShooting", true);
            
            Vector3 direction = GetDirection();

            if (Physics.Raycast(shootpos, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                TrailRenderer trail = Instantiate(BulletTrail, shootpos, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true, hit.collider));

                LastShootTime = ShootDelay;
            }
            // this has been updated to fix a commonly reported problem that you cannot fire if you would not hit anything
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, shootpos, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, shootpos + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = ShootDelay;
            }
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact, Collider col = null)
    {
        // This has been updated from the video implementation to fix a commonly raised issue about the bullet trails
        // moving slowly when hitting something close, and not
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        //Animator.SetBool("IsShooting", false);
        Trail.transform.position = HitPoint;
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal),currentTarget);
            if(col != null)
            {
                col.attachedRigidbody.GetComponent<CarFighter>().ApplyDamage(damage);
            }
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}