using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemainingBarController : MonoBehaviour
{
    public Transform[] quads;
    public Image fillbar;
    private float subAmount;

    public void SetBar(int amount)
    {
        fillbar.fillAmount = 1;
        subAmount = fillbar.fillAmount / amount;
        Debug.Log(subAmount);
        for (int i = 0; i < quads.Length; i++)
        {
            if (i < amount)
            {
                quads[i].gameObject.SetActive(true);
            }
            else
            {
                quads[i].gameObject.SetActive(false);
            }
        }
    }
    public void Substract()
    {
        //TODO: Agregar animacion 
        fillbar.fillAmount -= subAmount; 
    }
}
