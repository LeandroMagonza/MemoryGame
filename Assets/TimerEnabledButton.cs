using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerEnabledButton : MonoBehaviour
{
    public float timer = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0)
        {
            gameObject.GetComponent<Button>().interactable = true;
            return;
        }
        timer -= Time.deltaTime;
    }
}
