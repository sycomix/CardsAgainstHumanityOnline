﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandUI : MonoBehaviour
{
    [SerializeField] private CharacterHand _hand;
    [SerializeField] private GameObject _selector;
    [SerializeField] private List<Transform> _selectorPositions;
    [SerializeField] private List<UICard> _cards;

    private void Start()
    {
        _hand.OnSetNewCards += OnSetCards;
    }

    private void Update()
    {
        if (_hand == null) return;
        var pos = _selectorPositions[_hand.SelectorIndex].position;
        _selector.transform.position = new Vector2(pos.x, pos.y);
    }

    private void OnSetCards()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            if (i < _hand.Cards.Count)
            {
                _cards[i].text.text = _hand.Cards[i].Text;
            }
            else
            {
                _cards[i].Deactivate();
            }
        }
    }

    public void SetCharacterHand(CharacterHand characterHand)
    {
        _hand = characterHand;
        _hand.OnShowWhiteCards += ShowWhiteCards;
        _hand.OnHideWhiteCards += HideWhiteCards;
    }

    private void ShowWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Activate();
        }
    }

    private void HideWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Deactivate();
        }
    }
}