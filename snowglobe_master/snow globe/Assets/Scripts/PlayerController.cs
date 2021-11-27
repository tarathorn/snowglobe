using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;

    Vector3 lastPos;
    public GameObject player;
    // Start is called before the first frame update
void Start()
    {
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {

    if(player.transform.position != lastPos)
     {
         Walk();
     }
 
     else
     {
         animator.SetBool("IsWalking", false);
         animator.SetBool("wait", false);

     }
         lastPos = player.transform.position;
        
        
    if(Input.GetKeyDown(KeyCode.Space))
        {
            Wait();
        }

    }
     private void Walk()
    {

     animator.SetBool("IsWalking", true);
      if(Input.GetKeyDown(KeyCode.LeftShift))
     {
         slowWalk();
         animator.SetBool("IsWalking", false);

     }
 
     else
     {
         animator.SetBool("slowWalk", false);
         animator.SetBool("wait", false);

     }

    }

     private void slowWalk()
    {

     animator.SetBool("slowWalk", true);

    }
    
    private void Wait()
    {
          animator.SetBool("wait", true);
    }
    
    private void Slow()
    {
          animator.SetBool("slowWalk", true);
    }
}

