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
            public string avatarId;
            public string role;
        }

        public void AssignTeams()
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            Shuffle(avatars);

            int numCatchers = Mathf.Max(1, avatars.Count / 4);

            for (int i = 0; i < avatars.Count; i++)
            {
                string role = (i < numCatchers) ? "catcher" : "hider";
                var roleComp = avatars[i].GetComponent<Ubiq.Samples.AvatarRole>() ?? avatars[i].gameObject.AddComponent<Ubiq.Samples.AvatarRole>();
                roleComp.role = role;

                context.SendJson(new TeamAssignmentMessage
                {
                    avatarId = avatars[i].Peer?.uuid,
                    role = role
                });
                Debug.Log($"id:{avatars[i].Peer?.uuid},role:{role}");
            }
        }

        public void ProcessMessage(Ubiq.Messaging.ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<TeamAssignmentMessage>();
            foreach (var avatar in FindObjectsOfType<Ubiq.Avatars.Avatar>())
            {
                Debug.Log($"msgid:{msg.avatarId},msgrole:{msg.role},avatarid;{avatar.Peer?.uuid}");
                if (avatar.Peer?.uuid == msg.avatarId)
                {
                    var roleComp = avatar.GetComponent<Ubiq.Samples.AvatarRole>() ?? avatar.gameObject.AddComponent<Ubiq.Samples.AvatarRole>();
                    roleComp.role = msg.role;
                    break;
                }
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