using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectStageCanvas : MonoBehaviour
{
    public GameObject stageHolder;
    void OnEnable()
    {
        stageHolder.transform.localPosition = Vector3.zero;
    }
    public void CloseStickerPanel() {
        StageManager.Instance.CloseStickerPanel();
    }
}
