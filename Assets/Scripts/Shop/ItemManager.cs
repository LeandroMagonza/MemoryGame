using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ItemID
{
    NONE = -1,

    // Consumables

    Clue,
    Remove,
    Cut,
    Peek,

    /// Upgrdes

    ExtraLife,
    ProtectedLife,
    MaxClue,
    BetterClue,
    MaxRemove,
    MaxCut,
    BetterCut,
    BetterPeek,
    Block,
    DeathDefy
}

public class ItemManager : MonoBehaviour
{

    public Dictionary<ItemID, int> consumables = new Dictionary<ItemID, int>();
    public Dictionary<ItemID, int> matchInventory = new Dictionary<ItemID, int>();
    public Dictionary<ItemID, int> upgrades = new Dictionary<ItemID, int>();
    public static ItemManager Instance;

    [Header("Consumable Settings")]
    [SerializeField] private Button buttonClue;
    [SerializeField] private Button buttonRemove;
    [SerializeField] private Button buttonCut;
    [SerializeField] private Button buttonPeek;

    [SerializeField] private TextMeshProUGUI buttonTextClue;
    [SerializeField] private TextMeshProUGUI buttonTextRemove;
    [SerializeField] private TextMeshProUGUI buttonTextPeek;
    [SerializeField] private TextMeshProUGUI buttonTextCut;

    [SerializeField] private AudioClip buttonClueAudioClip;
    [SerializeField] private AudioClip buttonRemoveAudioClip;
    [SerializeField] private AudioClip buttonCutAudioClip;
    [SerializeField] private AudioClip buttonPeekAudioClip;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ValidateItem(ItemID.NONE);
    }
    public void AddItem(ItemID item)
    {
        if ((int)item < 0) return;
        Debug.Log("Added Item:" + nameof(item));
        AddItemTodictionaries(item);
        ValidateItem(item);
    }

    private void AddItemTodictionaries(ItemID item)
    {
        if ((int)item > 3)
        {
            AddUpgrade(item);
        }
        else
        {
            AddConsumable(item);
        }
    }

    private void AddConsumable(ItemID consumable)
    {
        if (consumables.ContainsKey(consumable))
        {
            consumables[consumable] += 1;
        }
        else
        {
            consumables.Add(consumable, 1);
        }
    }
    private void AddUpgrade(ItemID upgrade)
    {
        if (upgrades.ContainsKey(upgrade))
        {
            upgrades[upgrade] += 1;
        }
        else
        {
            upgrades.Add(upgrade, 1);
        }
    }
    public void UseClue()
    {
        Debug.Log("USE CLUE");

        if (!consumables.ContainsKey(ItemID.Clue) || consumables[ItemID.Clue] == 0) return;
        GameManager.Instance.audioSource.PlayOneShot(buttonClueAudioClip);
        //anim
        consumables[ItemID.Clue]--;
        if (consumables[ItemID.Clue] <= 0)
        {
            consumables[ItemID.Clue] = 0;
        }
        ValidateItem(ItemID.Clue);
        GameManager.Instance.OnCorrectGuess();
        GameManager.Instance.NextTurn();
    }
    public void UseRemove()
    {
        Debug.Log("USE REMOVE");
        if (!consumables.ContainsKey(ItemID.Remove) || consumables[ItemID.Remove] == 0) return;
        GameManager.Instance.audioSource.PlayOneShot(buttonRemoveAudioClip);
        //anim
        consumables[ItemID.Remove]--;
        if (consumables[ItemID.Remove] <= 0)
        {
            consumables[ItemID.Remove] = 0;
        }
        ValidateItem(ItemID.Remove);
        GameManager.Instance.RemoveStickerFromPool();
        GameManager.Instance.NextTurn();
    }
    private void Cut()
    {
    }
    private void Peek()
    {
    }
    public int GetItemPrice(ItemID item)
    {
        switch (item)
        {
            case ItemID.Clue:
                return 100;


            case ItemID.Remove:
                return 100;
            //case ItemID.Cut:
            //    break;
            //case ItemID.Peek:
            //    break;

            //// Upgrades
            //case ItemID.ExtraLife:
            //    break;
            //case ItemID.ProtectedLife:
            //    break;
            //case ItemID.MaxClue:
            //    break;
            //case ItemID.BetterClue:
            //    break;
            //case ItemID.MaxRemove:
            //    break;
            //case ItemID.MaxCut:
            //    break;
            //case ItemID.BetterCut:
            //    break;
            //case ItemID.BetterPeek:
            //    break;
            //case ItemID.Block:
            //    break;
            //case ItemID.DeathDefy:
            //    break;

            default:
                return 100;
        }

    }
    public void ValidateItem(ItemID itemId)
    {
        SetInteractable();
        SetButtonText();
    }
    private void SetButtonText()
    {
        if (buttonTextClue != null && consumables.ContainsKey(ItemID.Clue))
            buttonTextClue.text = consumables?[ItemID.Clue].ToString();
        if (buttonTextPeek != null && consumables.ContainsKey(ItemID.Peek))
            buttonTextPeek.text = consumables?[ItemID.Peek].ToString();
        if (buttonTextCut != null && consumables.ContainsKey(ItemID.Cut))
            buttonTextCut.text = consumables?[ItemID.Cut].ToString();
        if (buttonTextRemove != null && consumables.ContainsKey(ItemID.Remove))
            buttonTextRemove.text = consumables?[ItemID.Remove].ToString();
    }
    private void SetInteractable()
    {
        if (buttonClue != null && consumables.ContainsKey(ItemID.Clue))
            buttonClue.interactable = consumables?[ItemID.Clue] > 0;
        if (buttonRemove != null && consumables.ContainsKey(ItemID.Remove))
            buttonRemove.interactable = consumables?[ItemID.Remove] > 0;
        if (buttonCut != null && consumables.ContainsKey(ItemID.Cut))
            buttonCut.interactable = consumables?[ItemID.Cut] > 0;
        if (buttonPeek != null && consumables.ContainsKey(ItemID.Clue))
            buttonPeek.interactable = consumables?[ItemID.Peek] > 0;
    }
    public void UseItem(ItemID itemId)
    {
        switch (itemId)
        {
            case ItemID.Clue:
                UseClue();
                break;
            case ItemID.Remove:
                UseRemove();
                break;
            //case ItemID.Cut:
            //    break;
            //case ItemID.Peek:
            //    break;

            //// Upgrades
            //case ItemID.ExtraLife:
            //    break;
            //case ItemID.ProtectedLife:
            //    break;
            //case ItemID.MaxClue:
            //    break;
            //case ItemID.BetterClue:
            //    break;
            //case ItemID.MaxRemove:
            //    break;
            //case ItemID.MaxCut:
            //    break;
            //case ItemID.BetterCut:
            //    break;
            //case ItemID.BetterPeek:
            //    break;
            //case ItemID.Block:
            //    break;
            //case ItemID.DeathDefy:
            //    break;

            default:
                break;
        }
    }

}