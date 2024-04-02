using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public void CloseTutorialPanel()
    {
        GameManager.Instance.CloseTutorialPanel();
    }
}
