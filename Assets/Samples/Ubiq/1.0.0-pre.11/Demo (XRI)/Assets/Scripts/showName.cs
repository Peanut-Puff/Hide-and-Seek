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
        public List<TextMeshProUGUI> nameTexts;
        private List<Ubiq.Avatars.Avatar> avatars;
        public GameObject nameTextPrefab;
        public Transform namePanel;  
        private Dictionary<string, GameObject> nameObjects = new Dictionary<string, GameObject>(); 

        private void Start()
        {
            nameTexts[0].text = "waiting for start";
            for (int i =1; i < nameTexts.Count; i++)
            {

                nameTexts[i].gameObject.SetActive(false);
            }
            context = NetworkScene.Register(this);

        }
        public void StartLink()
        {
            avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            foreach (var avatar in avatars)
            {
                Debug.Log(avatar.Peer);
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
                    if (avatar != null) // NullReferenceException
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
                nameTexts[0].enabled = false;
                return;
            }
            int count = 0;
            foreach (var avatar in avatars)
            {
                if (avatar != null && avatar.Peer != null)
                {
                    var roleComp = avatar.GetComponent<AvatarRole>();
                    var myRole = roleComp.role;

                    string name = avatar.Peer[DisplayNameManager.KEY];

                    nameTexts[count].gameObject.SetActive(true);
                    nameTexts[count].text = name+" : "+myRole;
                    //nameTexts[count].enabled = true;
                    count += 1;
                }
            }
            for (int i=count; i < nameTexts.Count; i++)
            {
                nameTexts[i].gameObject.SetActive(false);
            }
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            if (!enabled) return;
            UpdateName();
        }
    }
}
