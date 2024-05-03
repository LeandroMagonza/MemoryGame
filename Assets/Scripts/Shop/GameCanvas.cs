using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] public GameObject pausePanel;
    public GameCanvasHeader gameCanvasHeader;
    public StageGroupIntroPanel stageGroupIntroPanel;
    public RemainingBarController barController => gameCanvasHeader.barController;
    public PowerPanelButtonHolder powerPanelButtonHolder;
    public Numpad numpad;
    
    private void OnEnable()
    {
        if (GameManager.Instance.startScale != Vector3.zero)
            GameManager.Instance.stickerDisplay.spriteHolder.transform.localScale = GameManager.Instance.startScale;
        UpdateUI();
    }

    public void UpdateUI()
    {
        powerPanelButtonHolder.SetAllPowerButtonsText();
        numpad.SetNumpadButtons(GameManager.Instance.GetCurrentlySelectedSticker());
    }
}

