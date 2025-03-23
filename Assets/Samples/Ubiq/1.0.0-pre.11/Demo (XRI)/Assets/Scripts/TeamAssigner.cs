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
        [System.Serializable]
        public struct PlayerRole
        {
            public string uuid;
            public string role;
        }

        [System.Serializable]
        private struct TeamAssignmentMessage
        {
            public List<PlayerRole> assignments;
        }

        public void AssignTeams()
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            Shuffle(avatars);

            int numCatchers = Mathf.Max(1, avatars.Count / 4);
            var assignments = new List<PlayerRole>();
            for (int i = 0; i < avatars.Count; i++)
            {
                string role = (i < numCatchers) ? "catcher" : "hider";
                var roleComp = avatars[i].GetComponent<AvatarRole>() ?? avatars[i].gameObject.AddComponent<AvatarRole>();
                roleComp.role = role;

                assignments.Add(new PlayerRole
                {
                    uuid = avatars[i].Peer?.uuid,
                    role = role
                });
            }
            context.SendJson(new TeamAssignmentMessage { assignments = assignments });
        }

        public void ProcessMessage(Ubiq.Messaging.ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<TeamAssignmentMessage>();
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            foreach (var assignment in msg.assignments)
            {
                foreach (var avatar in avatars)
                {
                    if (avatar.Peer?.uuid == assignment.uuid)
                    {
                        var roleComp = avatar.GetComponent<Ubiq.Samples.AvatarRole>() ?? avatar.gameObject.AddComponent<Ubiq.Samples.AvatarRole>();
                        roleComp.role = assignment.role;
                        break;
                    }
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