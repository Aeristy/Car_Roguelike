using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CarFighter : MonoBehaviour
{
    public float Durabilty = 100;
    public float DamageMul = 1;
    public LayerMask damageToLayer;
    public bool isColliding = false;
    public GameObject carArrow;
    public GameObject ExplodePrefab;
    public bool isDestroyed = false;
    private float deSpawnTime = 30;
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (isColliding) return;
    //    if (collision.gameObject.layer != (int)Mathf.Log(damageToLayer.value, 2)) return;      
    //    ApplyDamage(collision);
    //}
    private void Update()
    {
        if (isDestroyed)
        {
            deSpawnTime -= Time.deltaTime;
            if(deSpawnTime < 0)
                Destroy(gameObject);
        }

    }
    public void ApplyDamage(Collision col)
    {

        isColliding = true;
        StartCoroutine(ResetColliding());
        Vector3 impactSpeed = col.relativeVelocity * Mathf.Abs(Vector3.Dot(col.relativeVelocity.normalized, GetComponent<Rigidbody>().velocity.normalized));
        float damageToDuration = impactSpeed.magnitude;
        col.rigidbody.GetComponent<CarFighter>();
    }
    public void ApplyDamage(int damage)
    {
        Durabilty -= damage;
        if (Durabilty < 0)
        {
            Durabilty = 0;
            OnBroken();
        }
    }
    public void OnBroken()
    {
        CarAI car = GetComponent<CarAI>();
        car.enabled = false;
        car.frontLeft.enabled = false;
        car.frontRight.enabled = false;
        car.backLeft.enabled = false;
        car.backRight.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = 1;
        rb.AddForce(Vector3.up * 3, ForceMode.Impulse);
        GetComponent<BoxCollider>().enabled = true;
        gameObject.layer = 0;
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
            t.gameObject.layer = 0;
        carArrow.gameObject.SetActive(false);
        isDestroyed = true;
        Instantiate(ExplodePrefab,transform);
        
    }
    public IEnumerator ResetColliding()
    {
        yield return new WaitForSeconds(2f);
        isColliding = false;
    }
}
