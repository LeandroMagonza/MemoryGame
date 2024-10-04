using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTimedToggler : MonoBehaviour
{
    public float timer;
    public GameObject gameObjectToToggle;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer < 0) return;
        timer -= Time.deltaTime;
        if (timer < 0) {
            gameObjectToToggle.SetActive(!gameObjectToToggle.activeInHierarchy);
            CustomDebugger.Log("Toggling gameobject");
        }
    }
}
