using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{

    [SerializeField]
    int x, y;

    Ball ball_parent;

    void Start()
    {
        ball_parent = GetComponentInParent<Ball>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Ball"))
        {
            ball_parent.Collide(collider, x, y);
        }

        if(collider.CompareTag("Wall"))
        {
            ball_parent.Wall(collider);
        }
    }
}
