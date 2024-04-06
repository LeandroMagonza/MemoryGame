using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] public GameObject pausePanel;
    public GameCanvasHeader gameCanvasHeader;
    public StageGroupIntroPanel stageGroupIntroPanel;
    public RemainingBarController barController => gameCanvasHeader.barController;
    #region Singleton
    private static GameCanvas _instance;
    public static GameCanvas Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<GameCanvas>();
            if (_instance == null)
                CustomDebugger.LogError("Singleton<" + typeof(GameCanvas) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as GameCanvas;
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
    private void OnEnable()
    {
        if (GameManager.Instance.startScale != Vector3.zero)
            GameManager.Instance.stickerDisplay.spriteHolder.transform.localScale = GameManager.Instance.startScale;
    }
}
[System.Serializable]
public struct ConsumableButtonData
{
    [SerializeField] public string name;
    [SerializeField] public ConsumableID ID;
    [SerializeField] public Button button;
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public GameClip gameClip;
    [SerializeField] public GameObject peekCanvas;
    public void SetButtonText(ConsumableID consumableID)
    {
        if (text != null)
        {

            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {
                text.text = GameManager.Instance.matchInventory[consumableID].current.ToString();
            }
            else
            {
                CustomDebugger.Log("not implemented");
                text.text = "0";
            }
        }
        text.color = text.text.Equals("0") ? Color.white : Color.yellow;
    }
    public void SetInteractable(ConsumableID consumableID)
    {
        if (button != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {

                button.interactable = GameManager.Instance.matchInventory[consumableID].current > 0;
            }
            else
            {
                button.interactable = false;
            }
            button.GetComponent<Image>().color = button.interactable ? Color.white : Color.gray;
        }
    }


}
