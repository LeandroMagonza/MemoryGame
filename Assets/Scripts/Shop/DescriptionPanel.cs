using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionPanel : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI current;
    public TextMeshProUGUI aditionalInfo;
    public Button buyButton;



    public void SetUserInterface()
    {
        title.text = "Title";
        description.text = "Description";
        current.text = "Default";
        aditionalInfo.text = "Default";
        //cut
        //mejorar vida
    }
}
