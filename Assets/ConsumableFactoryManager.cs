using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableFactoryManager : MonoBehaviour
{
    #region Singleton
    private static ConsumableFactoryManager _instance;
    public static ConsumableFactoryManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<ConsumableFactoryManager>();
            if (_instance == null)
                CustomDebugger.LogError("Singleton<" + typeof(ConsumableFactoryManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as ConsumableFactoryManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    

    #endregion
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
