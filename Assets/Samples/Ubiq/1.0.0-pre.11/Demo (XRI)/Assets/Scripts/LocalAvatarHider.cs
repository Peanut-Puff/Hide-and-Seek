using UnityEngine;

public class LocalAvatarHider : MonoBehaviour
{
    private void Start()
    {
        int layer = LayerMask.NameToLayer("LocalAvatar");

        Camera cam = Camera.main;
        if (cam != null && layer >= 0)
        {
            cam.cullingMask &= ~(1 << layer);
            Debug.Log("LocalAvatar layer hidden from main camera.");
        }
        else
        {
            Debug.LogWarning("LocalAvatarHider: Camera or layer not found.");
        }
    }
}