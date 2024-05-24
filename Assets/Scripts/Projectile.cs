using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;
    private float targetAngle = 0.0f;
    private float radians = 0.0f;
    private Vector3 velocity;
    private bool firstHit = false;
    
    private void Start()
    {
        targetAngle = transform.rotation.z + 90.0f;
        radians = targetAngle * Mathf.Deg2Rad;
        velocity = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * speed;

    }

    private void Update()
    {
        transform.Translate(velocity * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Tank"))
        {
            if(!other.gameObject.GetComponent<Tank>().Active)
                return;
            
            if (firstHit)
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
            else
                firstHit = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
