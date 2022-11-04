﻿using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CharacterHand : MonoBehaviourPun
{
    [SerializeField] private List<CardModel> _cards;

    [SerializeField] private int _selectorIndex;

    private int _selectedCard;
    
    public int SelectorIndex => _selectorIndex;
    public void MoveSelectorRight()
    {
        _selectorIndex = Math.Min(_cards.Count-1, _selectorIndex+1);
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
    }

    public void MoveSelectorLeft()
    {
        _selectorIndex = Math.Max(0, _selectorIndex-1);
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
    }

    [PunRPC]
    private void UpdateSelectorIndex(int index)
    {
        _selectorIndex = index;
    }

    public void SelectCard()
    {
        if (_selectedCard > 0 && _selectedCard == _selectorIndex) _selectedCard = -1;
        else _selectedCard = _selectorIndex;
    }

    public CardModel GetSelectedCard()
    {
        if (_selectedCard < 0) return null;
        return _cards[_selectorIndex];
    }
    
}