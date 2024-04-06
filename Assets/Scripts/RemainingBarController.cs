using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RemainingBarController : MonoBehaviour {
    public RemainingStickerMarker remainingStickerMarker;
    public List<RemainingStickerMarker> markers = new ();
    public int currentCount = 0;
    public GameObject markerContainer;
    public void SetBar(int amount)
    {
        CustomDebugger.Log("called setbar with amount "+amount);
        currentCount = 0;
        foreach (var marker in markers.ToList()) {
            if (marker is not null) Destroy(marker.gameObject);
        }
        markers = new ();

        CustomDebugger.Log("markers = "+markers.Count);
        RemainingStickerMarker lastRemainingStickerMarker = null;
        for (int i = 0; i < amount; i++)
        {
            if (markers.Count <= i) {
                RemainingStickerMarker newMarker = Instantiate(remainingStickerMarker, markerContainer.transform);
                markers.Add(newMarker);
            }
            CustomDebugger.Log("marker"+i+ " set active");
            markers[i].gameObject.SetActive(true);
            markers[i].SetText((i).ToString());
            markers[i].DisplayText(false);
            lastRemainingStickerMarker = markers[i];
        }
        
        markers[0].DisplayText(true);
        lastRemainingStickerMarker?.DisplayText(true);
        
    }
    public void IncreaseCounter()
    {
        //TODO: Agregar animacion 
        markers[currentCount].DisplayText(false);
        if (markers.Count > currentCount+1) {
            markers[currentCount+1].DisplayText(true);
        }
        currentCount++;
    }
}
