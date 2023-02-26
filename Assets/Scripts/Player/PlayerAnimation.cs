using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Rigidbody rb;
    public Animator pcAnimator;

    private void Start() 
    {
        rb = GetComponent<Rigidbody>();
        pcAnimator = GetComponentInChildren<Animator>();
    }

    public void Update() 
    {
        // Still thinking about better locomotion system but let's leave it as it is
        if ( Mathf.Abs(rb.velocity.x) > .1f || Mathf.Abs(rb.velocity.z) > .1f)
        {
            pcAnimator.SetBool("isRunning", true);
        }
        else
        pcAnimator.SetBool("isRunning", false);
    }
}
