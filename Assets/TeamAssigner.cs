using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;

public class TeamAssigner : MonoBehaviour
{
    public void AssignTeams()
    {
        var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
        Shuffle(avatars);

        int numCatchers = Mathf.Max(1, avatars.Count / 4);

        for (int i = 0; i < avatars.Count; i++)
        {
            string role = (i < numCatchers) ? "catcher" : "hider";
            var roleComp = avatars[i].GetComponent<AvatarRole>() ?? avatars[i].gameObject.AddComponent<AvatarRole>();
            roleComp.role = role;
            Debug.Log($"{role}");
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