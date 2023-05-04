using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    public float speed = 15f;
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    private Rigidbody rb;
    public GameObject[] Detached;
    public BulletPoolController bulletPool;
    private IEnumerator iEnumerator;

    private ParticleSystem bulletParticle;
    private Collider col;

    private void Awake()
    {
        bulletPool = null;

        bulletParticle = GetComponent<ParticleSystem>();
        col = GetComponent<Collider>();
        col.enabled = false;
        if(transform.childCount > 1)
        {
            flash = transform.GetChild(1).gameObject;
            hit = transform.GetChild(2).gameObject;
            hit.SetActive(false);
            flash.SetActive(false);
        }
        
        //hit = transform.GetChild(2).gameObject;

    }

    public void shot()
    {
        speed = 15f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        col.enabled = true;
        if (flash != null)
        {
            
            //Instantiate flash effect on projectile position
            //var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
            var flashInstance = flash;
            flashInstance.transform.parent = null;
            //hit.transform.parent = null;

            flashInstance.transform.position = gameObject.transform.position;
            flashInstance.transform.forward = gameObject.transform.forward;
            flash.SetActive(true);
            bulletParticle.Play();
            //Destroy flash effect depending on particle Duration time
            
            ParticleSystem[] flashPss = flashInstance.GetComponentsInChildren<ParticleSystem>();
            foreach(ParticleSystem ps in flashPss)
            {
                ps.Play();
            }

           // var flashPs = flashInstance.GetComponent<ParticleSystem>();
            //flashPs.Play();
            //if (flashPs != null)
            //{
            //    // Destroy(flashInstance, flashPs.main.duration);
            //   // StartCoroutine(SetParent(flashInstance.transform, flashPs.main.duration));
            //}
            //else
            //{
            //    var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
            //   // StartCoroutine(SetParent(flashInstance.transform, flashPsParts.main.duration));
            //    //Destroy(flashInstance, flashPsParts.main.duration);
            //}
        }
        if(iEnumerator == null)
        {
            iEnumerator = PushToPool(5.0f);
            StartCoroutine(iEnumerator);
        }
        //Invoke("PushToPool", 5.0f);
    }

    IEnumerator SetParent(Transform tr, float delay)
    {
        Debug.Log("setparent");
        yield return new WaitForSeconds(delay);
        tr.parent = gameObject.transform;
    }

    void Start()
    {
        
       //Destroy(gameObject,5);
	}

    void FixedUpdate ()
    {
		if (speed != 0)
        {
            rb.velocity = transform.forward * speed;
            //transform.position += transform.forward * (speed * Time.deltaTime);         
        }
	}

    //https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
    void OnCollisionEnter(Collision collision)
    {
        //Lock all axes movement and rotation
        bulletParticle.Clear();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        col.enabled = false;
        speed = 0;
        float duration = 0;
       //Debug.Log("hit" + " : " + collision.gameObject.name);

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;

        //Spawn hit effect on collision
        if (hit != null)
        {
            //var hitInstance = Instantiate(hit, pos, rot);
            
            var hitInstance = hit;
            hitInstance.transform.parent = null;
            hitInstance.transform.position = pos;
            hitInstance.transform.rotation = rot;
            hit.SetActive(true);
            

            ParticleSystem[] hitPss = hitInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in hitPss)
            {
                ps.Play();
            }

            if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
            else { hitInstance.transform.LookAt(contact.point + contact.normal); }

            //Destroy hit effects depending on particle Duration time
            var hitPs = hitInstance.GetComponent<ParticleSystem>();
           // hitPs.Play();
            if (hitPs != null)
            {
                // Destroy(hitInstance, hitPs.main.duration);
                duration = hitPs.main.duration;
               // StartCoroutine(SetParent(hitInstance.transform, hitPs.main.duration));
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                duration = hitPsParts.main.duration;
              //  StartCoroutine(SetParent(hitInstance.transform, hitPsParts.main.duration));
               // Destroy(hitInstance, hitPsParts.main.duration);
            }
        }

        //Removing trail from the projectile on cillision enter or smooth removing. Detached elements must have "AutoDestroying script"
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }

        if (iEnumerator == null)
        {
            iEnumerator = PushToPool(duration);
            StartCoroutine(iEnumerator);
        }
        else
        {
            StopCoroutine(iEnumerator);
            StartCoroutine(PushToPool(duration));
        }
        //Destroy projectile on collision
       // Destroy(gameObject);
    }

    IEnumerator PushToPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hit.transform.parent != this.transform)
        {
            hit.SetActive(false);
            hit.transform.parent = this.transform;
        }
        if (flash.transform.parent != this.transform)
        {
            flash.SetActive(false);
            flash.transform.parent = this.transform;
        }

        string name = this.name.Replace("(Clone)", "");
        bulletPool.PushToPool(name, this.gameObject);
        iEnumerator = null;
    }
}
