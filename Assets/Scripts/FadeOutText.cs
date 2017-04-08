using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeOutText : MonoBehaviour
{
    public float InActiveAlpha;
    public Text Text;

    private bool _fadeOut;
    private Color color;
    private float alpha;
    public float FadeoutSpeed = 1;
    void Start()
    {
        Text.color = new Color(1,1,1,1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            FadeOut();

        if (_fadeOut && alpha > InActiveAlpha)
        {
            alpha -= Time.deltaTime*FadeoutSpeed;
            color = new Color(1,1,1, alpha);
            Text.color = color;
        }
    }
    public void FadeOut()
    {
        alpha = 255;
        _fadeOut = true;
        color = new Color(1, 1, 1, 1);
    }
}
