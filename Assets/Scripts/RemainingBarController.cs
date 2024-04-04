using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemainingBarController : MonoBehaviour {
    public RemainingStickerMarker remainingStickerMarker;
    public List<RemainingStickerMarker> markers;
    public int currentCount = 0;
    public GameObject markerContainer;
    public void SetBar(int amount)
    {
        CustomDebugger.Log("called setbar");
        currentCount = 0;
        foreach (var marker in markers) {
            marker.gameObject.SetActive(false);
        }

        CustomDebugger.Log("markers = "+markers.Count);
        RemainingStickerMarker lastRemainingStickerMarker = markers[0];
        for (int i = 0; i < amount; i++)
        {
            if (markers.Count <= i) {
                markers.Add(Instantiate(remainingStickerMarker,markerContainer.transform));
            }
            CustomDebugger.Log("marker"+i+ " set active");
            markers[i].gameObject.SetActive(true);
            markers[i].SetText((i+1).ToString());
            markers[i].DisplayText(false);
            lastRemainingStickerMarker = markers[i];
        }
        
        markers[0].DisplayText(true);
        lastRemainingStickerMarker.DisplayText(true);
        
    }
    public void IncreaseCounter()
    {
        //TODO: Agregar animacion 
        markers[currentCount].DisplayText(false);
        if (markers.Count > currentCount) {
            markers[currentCount+1].DisplayText(true);
        }
        currentCount++;
    }
}
