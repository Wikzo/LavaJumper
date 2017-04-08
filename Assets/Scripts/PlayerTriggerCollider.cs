using UnityEngine;
using System.Collections;

public class PlayerTriggerCollider : MonoBehaviour {

    public string PowerupTag = "Powerup";
    public string KillFloor = "KillFloor";
    public RaycastForward RaycastForward;

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag(PowerupTag))
        {
            RaycastForward.PowerUp();
            Destroy(c.gameObject);
        }

        else if (c.CompareTag(KillFloor))
            RaycastForward.ResetGame();
    }
}
