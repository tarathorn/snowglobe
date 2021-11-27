using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerEnter : MonoBehaviour
{
 
    [SerializeField] private Animator motionCheckBox;
    [SerializeField] private string upanimation =  "updown";


    private void onTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            motionCheckBox.Play(upanimation, 0, 0.0f);
            Debug.Log("hit");
        }
    }

}
