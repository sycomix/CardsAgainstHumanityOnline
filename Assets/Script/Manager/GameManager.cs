﻿using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPun
{
    [SerializeField] private TextAsset _blackCardsJson;
    [SerializeField] private TextAsset _whiteCardsJson;
    
    [SerializeField] private List<RoundAction> _roundActions;
    [SerializeField] private BlackCardModel _blackCard;
    [SerializeField] private WinnerWhiteCardModel _winnerWhiteCard;
    [SerializeField] private JudgeModel _judge;
    
    [Range(1, 10)] [SerializeField] private int _winCondition = 3;
    
    private Queue<RoundAction> _roundActionsQueue = new Queue<RoundAction>();
    private RoundAction _currentRoundAction;

    private List<CharacterModel> _characters;
    private int _currentJudgeIndex;

    private string[] _blackCardsStrings;
    private string[] _whiteCardsStrings;

    private Stack<string> _blackCards = new Stack<string>();
    private Stack<string> _whiteCards = new Stack<string>();

    private string _winnerCard;
    private string _winnerNickname;
    
    public int CurrentJudgeIndex => _currentJudgeIndex;
    public List<CharacterModel> Characters => _characters;

    public Dictionary<CardModel, CharacterModel> SelectedCards { get; set; }

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Destroy(gameObject);
            return;
        }
        
        Destroy(Camera.main.gameObject);
        
        _blackCardsStrings = JsonUtility.FromJson<Cards>(_blackCardsJson.text).cards;
        _whiteCardsStrings = JsonUtility.FromJson<Cards>(_whiteCardsJson.text).cards;
        
        Debug.Log($"White cards loaded: {_whiteCardsStrings.Length}");
        Debug.Log($"Black cards loaded: {_blackCardsStrings.Length}");
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        MasterManager.Instance.OnChangeBlackCard += SetNewBlackCard;
        MasterManager.Instance.OnChangeMyWhiteCards += SetNewWhiteCards;
        MasterManager.Instance.OnChangeAllWhiteCards += SetNewWhiteCards;
        
        EnqueueRoundActions();
        SetNewJudgeIndex();
        RoundActionEnded();
    }

    private void RoundActionEnded()
    {
        if (_roundActionsQueue.Count == 0)
        {
            ShowWinnerCard();
            RoundEnded();
        }
        
        SetCurrentRoundAction(_roundActionsQueue.Dequeue());
    }

    private void RoundEnded()
    {
        foreach (var character in _characters)
        {
            if (character.Points >= _winCondition) // Parametrize win condition value
            {
                Debug.Log($"Win condition met, winner is: {MasterManager.Instance.GetPlayerFromCharacter(character).NickName}");
                WinConditionMet(character);
                break;
            }
        }
        
        SetNewWhiteCards();

        SetNewBlackCard();
        SetNewJudgeIndex();
        EnqueueRoundActions();
    }

    private void WinConditionMet(CharacterModel winner)
    {
        var winCondition = new Hashtable();
        winCondition.Add("WinCondition", _winCondition);
        PhotonNetwork.CurrentRoom.SetCustomProperties(winCondition);
        foreach (var characterModel in _characters)
        {
            var player = MasterManager.Instance.GetPlayerFromCharacter(characterModel);
            player.SetScore(characterModel.Points);
            MasterManager.Instance.RPC(nameof(MasterManager.Instance.WinConditionMet) ,player,characterModel == winner);
        }
        Invoke(nameof(ChangeScene), 2f);
    }

    private void ChangeScene()
    {
        PhotonNetwork.LoadLevel("ScoreScreen");
    }

    private void LoadCards()
    {
        LoadBlackCards();
        LoadWhiteCards();
    }

    private void LoadBlackCards()
    {
        if (_blackCardsStrings == null || _blackCardsStrings.Length <= 0)
            _blackCardsStrings = JsonUtility.FromJson<Cards>(_blackCardsJson.text).cards;
        ShuffleDeck(ref _blackCardsStrings);
        
        _blackCards = new Stack<string>(_blackCardsStrings);
    }

    private void LoadWhiteCards()
    {
        if (_whiteCardsStrings == null || _whiteCardsStrings.Length <= 0)
            _whiteCardsStrings = JsonUtility.FromJson<Cards>(_whiteCardsJson.text).cards;
        ShuffleDeck(ref _whiteCardsStrings);
        
        _whiteCards = new Stack<string>(_whiteCardsStrings);
    }

    private void ShuffleDeck(ref string[] cards)
    {
        for (int i = 0; i < cards.Length; i++) {
            string temp = cards[i];
            int randomIndex = Random.Range(i, cards.Length);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    private void SetCurrentRoundAction(RoundAction roundAction)
    {
        if (_currentRoundAction != null) _currentRoundAction.OnEndRoundAction = delegate {};
        
        _currentRoundAction = roundAction;

        _currentRoundAction.OnEndRoundAction += RoundActionEnded;
        _currentRoundAction.StartRoundAction();
    }

    private void EnqueueRoundActions()
    {
        foreach (var roundAction in _roundActions)
        {
            _roundActionsQueue.Enqueue(roundAction);
        }
    }

    private void SetNewJudgeIndex()
    {
        if (_currentJudgeIndex >= _characters.Count - 1) _currentJudgeIndex = 0;
        else _currentJudgeIndex++;
        
        _judge.SetNickname(MasterManager.Instance.GetPlayerFromCharacter(_characters[_currentJudgeIndex]).NickName);
    }

    public void SetCharacters(List<CharacterModel> characters)
    {
        _characters = characters;
    }

    public void SetNewWhiteCards()
    {
        if (_whiteCards.Count <= 5)
        {
            LoadWhiteCards();
        }
        _characters.ForEach(c =>
        {
            List<string> newCards = new List<string>();
            for (int i = 0; i < 5; i++) newCards.Add(_whiteCards.Pop());
            c.Hand.SetCards(newCards);
        });
    }

    public void SetNewWhiteCards(Player client)
    {
        SetNewWhiteCards(MasterManager.Instance.GetCharacterModelFromPlayer(client));
    }
    
    public void SetNewWhiteCards(CharacterModel character)
    {
        List<string> newCards = new List<string>();
        for (int i = 0; i < 5; i++) newCards.Add(_whiteCards.Pop());
        character.Hand.SetCards(newCards);
    }

    public void SetNewBlackCard()
    {
        if (_blackCards.Count <= 0)
        {
            LoadBlackCards();
        }
        
        _blackCard.SetText(_blackCards.Pop());
        _blackCard.SetShowCard(3f);
    }

    public void SetWinnerCard(string winnerCard, string nickname)
    {
        _winnerCard = winnerCard;
        _winnerNickname = nickname;
    }

    public void ShowWinnerCard()
    {
        _winnerWhiteCard.SetText(_winnerCard, _winnerNickname);
        _winnerWhiteCard.SetShowCard();
    }
    

    public CharacterModel GetCurrentJudge()
    {
        Debug.Log($"Current characters: {_characters.Count} & current judge index: {_currentJudgeIndex}");
        return _characters[_currentJudgeIndex];
    }
}

[Serializable]
public class Cards
{
    public string[] cards;
}