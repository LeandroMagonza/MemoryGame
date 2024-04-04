using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButtonPanel : MonoBehaviour
{
    public void PauseButtonPressed() {
        GameManager.Instance.TogglePause();
    }
}
