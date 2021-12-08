using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance;
    
    public Image OwnProgressBar;

    public Image EnemyProgressBar1;
    public Image EnemyProgressBar2;
    public Image EnemyProgressBar3;

    public GameObject UIShoppingList;
    
    private const float AMOUNT_PER_ITEM = 0.2f;


    private Dictionary<ulong, Image> _playerBars;

    private bool _barsAlreadyInitialized;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _barsAlreadyInitialized = false;
        if (PlayerProgress.Instance == null)
        {
            Instance = this;
        }

        _playerBars = new Dictionary<ulong, Image>();
        
        MatchManager.OnMatchStart += InitializeBars;
        MatchManager.OnMatchExit += HideUIShoppingList;
        
    }

    private void OnDestroy()
    {
        MatchManager.OnMatchStart -= InitializeBars;
        MatchManager.OnMatchExit -= HideUIShoppingList;
    }
    
    private void InitializeBars()
    {
        UIShoppingList.SetActive(true);
        if (_barsAlreadyInitialized)
        {
            return;
        }

        OwnProgressBar.fillAmount = 0;
        EnemyProgressBar1.fillAmount = 0;
        EnemyProgressBar2.fillAmount = 0;
        EnemyProgressBar3.fillAmount = 0;
        
        int playerColor = NetworkController.SelfPlayer.GetComponent<PlayerInfo>().PlayerData.Color;
        OwnProgressBar.color = ColorManager.Instance.GetColor(playerColor).color;

        _playerBars.Add(NetworkController.SelfID, OwnProgressBar);
        
        int nextAvailableBar = 0;
        
        foreach (ulong playerId in NetworkController.GetLocalPlayers().Keys)
        {
            if (playerId != NetworkController.SelfID)
            {
                Image progressBar = GetNextAvailableBar(nextAvailableBar);
                playerColor = NetworkController.GetPlayerByID(playerId).GetComponent<PlayerInfo>().PlayerData.Color;
                progressBar.color = ColorManager.Instance.GetColor(playerColor).color;
                
                _playerBars.Add(playerId, progressBar);
                nextAvailableBar++;
            }
        }

        _barsAlreadyInitialized = true;
    }

    private Image GetNextAvailableBar(int nextAvailableBar)
    {
        switch (nextAvailableBar)
        {
            case 0:
                return EnemyProgressBar1;
            case 1:
                return EnemyProgressBar2;
            case 2:
                return EnemyProgressBar3;
        }

        return null;
    }

    public void AddItemToPlayer(ulong playerId)
    {
        _playerBars[playerId].fillAmount += AMOUNT_PER_ITEM;
    }
    
    public void RemoveItemToPlayer(ulong playerId)
    {
        _playerBars[playerId].fillAmount -= AMOUNT_PER_ITEM;
    }

    private void HideUIShoppingList()
    {
        UIShoppingList.SetActive(false);
        foreach (ulong playerId in _playerBars.Keys)
        {
            ClearBar(playerId);
        }
    }

    private void ClearBar(ulong playerId)
    {
        _playerBars[playerId].fillAmount = 0;
    }
}
