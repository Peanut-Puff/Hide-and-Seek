using Ubiq.Messaging;
using UnityEngine;
using TMPro;
using Ubiq.Rooms;
using System.Collections.Generic;
using Ubiq.Avatars;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class ShowName : MonoBehaviour
    {
        private NetworkContext context;
        public TextMeshProUGUI nameText;
        private List<Ubiq.Avatars.Avatar> avatars;

        private void Start()
        {
            nameText.text = "waiting for start";
            context = NetworkScene.Register(this);
            avatars = new List<Avatars.Avatar>(FindObjectsOfType<Avatars.Avatar>());
            if (avatars != null || avatars.Count == 0)
            {
                Debug.Log("avatars 是一个空列表，但不是 null");
            }
            foreach (var avatar in avatars)
            {
                Debug.Log(avatar.Peer); // 输出每个 Avatar 的名字
            }

            foreach (var avatar in avatars)
            {
                avatar.OnPeerUpdated.AddListener(Avatar_OnPeerUpdated);
            }

        }
        public void StartLink()
        {
            avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            foreach (var avatar in avatars)
            {
                Debug.Log(avatar.Peer); // 输出每个 Avatar 的名字
            }

            foreach (var avatar in avatars)
            {
                avatar.OnPeerUpdated.AddListener(Avatar_OnPeerUpdated);
            }
        }
        private void OnDestroy()
        {
            if (avatars != null)
            {
                foreach (var avatar in avatars)
                {
                    if (avatar != null) // 防止 NullReferenceException
                    {
                        avatar.OnPeerUpdated.RemoveListener(Avatar_OnPeerUpdated);
                    }
                }
            }
        }

        private void Avatar_OnPeerUpdated(IPeer peer)
        {
            UpdateName();
        }

        private void UpdateName()
        {
            if (avatars == null || avatars.Count == 0)
            {
                nameText.enabled = false;
                return;
            }

            foreach (var avatar in avatars)
            {
                if (avatar != null && avatar.Peer != null)
                {
                    string name = avatar.Peer[DisplayNameManager.KEY];

                    if (!string.IsNullOrEmpty(name))
                    {
                        nameText.text = name;
                        nameText.enabled = true;
                        return; 
                    }
                }
            }

            nameText.enabled = false; 
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            if (!enabled) return;
            UpdateName();
        }
    }
}
