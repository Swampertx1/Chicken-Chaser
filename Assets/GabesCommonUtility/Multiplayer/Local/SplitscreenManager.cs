using System;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.Local
{
    public class SplitscreenManager : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogError("SplitscreenManager needs to be re-made in a manner that works for both online and offline games.");
        }
    }
}