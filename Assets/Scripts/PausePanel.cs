using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
    // Start is called before the first frame update
    public void ContinueButtonPressed() {
        GameManager.Instance.TogglePause();
    }    
    public void RetryButtonPressed() {
        GameManager.Instance.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
