﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandUI : MonoBehaviour
{
    [SerializeField] private CharacterHand _hand;
    [SerializeField] private GameObject _selector;
    [SerializeField] private List<Transform> _selectorPositions;
    [SerializeField] private List<UICard> _cards;

    private void Update()
    {
        if (_hand == null) return;
        var pos = _selectorPositions[_hand.SelectorIndex].position;
        _selector.transform.position = new Vector2(pos.x, pos.y);
    }

    public void SetCharacterHand(CharacterHand characterHand)
    {
        _hand = characterHand;
        _hand.OnShowWhiteCards += ShowWhiteCards;
        _hand.OnHideWhiteCards += HideWhiteCards;
        _hand.OnSetNewCards += SetNewCards;

        for (int i = 0; i < _cards.Count; i++)
        {
            _hand.Cards[i].OnTextUpdated += _cards[i].UpdateText;
        }
        
        SetNewCards();
    }

    private void ShowWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Activate();
        }
        _selector.SetActive(true);
    }

    private void HideWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Deactivate();
        }
        _selector.SetActive(false);
    }

    private void SetNewCards()
    {
        Debug.Log("Updating hand UI cards!!!!!!!!!");
        // debug
        string debug = "";
        foreach (var card in _cards)
        {
            debug += $"{card}, ";
        }
        Debug.Log(debug);
        // end debug
        for (var i = 0; i < _cards.Count; i++)
        {
            if (_hand.Cards[i].IsActive)
            {
                _cards[i].text.text = _hand.Cards[i].Text;
                _cards[i].Activate();
            }
            else
            {
                _cards[i].Deactivate();
            }
        }
    }
}