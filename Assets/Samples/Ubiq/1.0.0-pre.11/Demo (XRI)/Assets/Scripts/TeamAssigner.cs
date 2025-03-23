using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
namespace Ubiq.Samples
{
    public class TeamAssigner : MonoBehaviour
    {
        private NetworkContext context;

        private void Start()
        {
            context = NetworkScene.Register(this);
        }

        private struct TeamAssignmentMessage
        {
            public IDictionary<string, string> roleDic;
        }

        public void AssignTeams()
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            Shuffle(avatars);

            int numCatchers = Mathf.Max(1, avatars.Count / 4);
            IDictionary<string, string> roleDic = new Dictionary<string, string>();
            for (int i = 0; i < avatars.Count; i++)
            {
                string role = (i < numCatchers) ? "catcher" : "hider";
                var roleComp = avatars[i].GetComponent<Ubiq.Samples.AvatarRole>() ?? avatars[i].gameObject.AddComponent<Ubiq.Samples.AvatarRole>();
                roleComp.role = role;
                roleDic.Add(avatars[i].Peer?.uuid,role);
            }
            context.SendJson(new TeamAssignmentMessage
            {
                roleDic=roleDic
            });
        }

        public void ProcessMessage(Ubiq.Messaging.ReferenceCountedSceneGraphMessage message)
        {
            IDictionary<string, string> roleDic = message.FromJson<TeamAssignmentMessage>().roleDic;
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            for (int i = 0; i < avatars.Count; i++){
                var roleComp = avatars[i].GetComponent<Ubiq.Samples.AvatarRole>() ?? avatars[i].gameObject.AddComponent<Ubiq.Samples.AvatarRole>();
                roleComp.role = roleDic[avatars[i].Peer?.uuid];
            }
        }

        private void Shuffle(List<Ubiq.Avatars.Avatar> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}