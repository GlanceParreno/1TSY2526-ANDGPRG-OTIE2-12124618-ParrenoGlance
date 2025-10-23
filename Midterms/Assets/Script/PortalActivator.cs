using UnityEngine;

public class PortalActivator : MonoBehaviour
{
    public GameObject portal;

    void Update()
    {
        if (GameManager.Instance.AllCoinsCollected() && !portal.activeSelf)
        {
            portal.SetActive(true);
        }
    }
}
