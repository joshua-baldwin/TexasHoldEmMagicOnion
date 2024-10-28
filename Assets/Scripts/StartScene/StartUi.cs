using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace THE.StartScene
{
    public class StartUi : MonoBehaviour
    {
        [SerializeField] private Button createRoom;
        [SerializeField] private Button joinRoom;

        private void Awake()
        {
            createRoom.onClick.AddListener(CreateRoom);
            joinRoom.onClick.AddListener(JoinRoom);
        }

        private void CreateRoom()
        {

        }

        private void JoinRoom()
        {

        }
    }
}