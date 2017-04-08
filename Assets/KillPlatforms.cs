using UnityEngine;
using System.Collections;

public class KillPlatforms : MonoBehaviour
{
    public LevelGenerator LevelGenerator;

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag(LevelGenerator.PlatformTag) ||
            c.gameObject.CompareTag(LevelGenerator.PowerupTag))
        {
            LevelGenerator.KillPlatform((c.gameObject));
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag(LevelGenerator.PlatformTag) ||
            c.gameObject.CompareTag(LevelGenerator.PowerupTag))
        {
            LevelGenerator.KillPlatform((c.gameObject));
        }
    }

}
