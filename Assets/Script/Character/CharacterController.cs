﻿using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CharacterController : MonoBehaviourPun
{
    [SerializeField] private CharacterModel _characterModel;

    public void Destroy()
    {
        Destroy(this);
    }

    private void Start()
    {
        CommunicationsManager.Instance.inputManager.OnReturnPressed += ReturnPressed;
        CommunicationsManager.Instance.inputManager.OnRightArrowPressed += RightArrowPressed;
        CommunicationsManager.Instance.inputManager.OnLeftArrowPressed += LeftArrowPressed;
        
        if (!PhotonNetwork.IsMasterClient) photonView.RPC(nameof(IsMine), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    private void LeftArrowPressed()
    {
        _characterModel.Move(false);
    }
    
    private void RightArrowPressed()
    {
        _characterModel.Move(true);
    }
    
    private void ReturnPressed()
    {
        _characterModel.SelectCard();
    }

    [PunRPC]
    private void IsMine(Player player)
    {
        if (MasterManager.Instance.GetPlayerFromCharacter(_characterModel).NickName != player.NickName)
        {
            photonView.RPC(nameof(RemoteDestroy), player);
        }
    }

    [PunRPC]
    private void RemoteDestroy()
    {
        Destroy();
    }
    
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, true);
        
        if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, false);
        
        if (Input.GetKeyDown(KeyCode.Return))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestSelect), PhotonNetwork.LocalPlayer);
    }*/
}