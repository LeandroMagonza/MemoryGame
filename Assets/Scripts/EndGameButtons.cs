using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameButtons : MonoBehaviour {
    public void Replay (){ GameManager.Instance.Reset();}
    
    public void PlayNextStage(){ GameManager.Instance.PlayNextStage();}
    
}
