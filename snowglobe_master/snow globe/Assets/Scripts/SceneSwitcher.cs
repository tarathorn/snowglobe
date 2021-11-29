using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
  private bool NearGlobe = false;
    void Start()
    {
    
    }

    void OnTriggerEnter (Collider other)

    {

         Debug.Log ("near globe is true..");
         NearGlobe = true;
        

    }

public void PlayCloseUp()

    {
        
     NearGlobe = false;
     SceneManager.LoadScene("Closeup");
        

    }
  
void Update()
  {
    if(NearGlobe == true && (Input.GetKeyDown("space")))
    {
      Debug.Log ("space pressed..");
       StartCoroutine(waiter());
    }
  }

    IEnumerator waiter()
  {
    
    Debug.Log ("start counting..");
    yield return new WaitForSeconds(4);
    PlayCloseUp();
  }   

}
