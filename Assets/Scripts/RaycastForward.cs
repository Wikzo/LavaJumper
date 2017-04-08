using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.ImageEffects;

public class RaycastForward : MonoBehaviour
{
    public Transform PlayerTransform;
    public Transform CameraTransform;
    public float RaycastDistance = 100;
    public LayerMask ObjectsToHit;
    public FirstPersonController FirstPersonController;
    public CharacterController CharacterController;

    public Camera PlayerCamera;
    public float TeleportingFOV = 80;
    private float _standardFOV;

    public Transform TargetIcon;
    private MeshRenderer _targeticonMeshRenderer;
    public LineRenderer LineRenderer;

    public bool CurrentlyTeleporting;

    public float TimeToMoveLerp = 1f;
    private bool _isCurrentlyResetting;

    public KeyCode SlowmoKey;
    public float SlowmoTimeScale = 0.5f;

    public int MaxNumberOfJumpsBeforeTouchingGround = 3;
    public int CurrentJumpsLeft;

    public AudioSource Music;
    public Text JumpsText;
    public Text HighScoreText;

    private float _highScore;

    private Vector3 _startPosition;

    private void Start()
    {
        GlitchEffect.enabled = false;

        _startPosition = PlayerTransform.position;
        Vignette.enabled = false;
        LineRenderer.enabled = false;
        _targeticonMeshRenderer = TargetIcon.GetComponent<MeshRenderer>();
        _standardFOV = PlayerCamera.fov;
        ResetTeleportJumps();
    }

    public VignetteAndChromaticAberration Vignette;
    public GlitchEffect GlitchEffect;
    public AudioSource AudioSource;
    public AudioClip GlitchEffectAudioClip;
    public float GlitchTime = 0.6f;
    public LevelGenerator LevelGenerator;
    public void ResetGame()
    {
        StartCoroutine(CameraGlitchEffectEnable(GlitchTime));
    }

    IEnumerator CameraGlitchEffectEnable(float time)
    {
        GlitchEffect.enabled = true;
        LevelGenerator.Reset();

        CharacterController.SimpleMove(Vector3.zero);
        CharacterController.enabled = false;
        yield return new WaitForSeconds(0.2f);
        AudioSource.PlayOneShot(GlitchEffectAudioClip);

        PlayerTransform.position = _startPosition;
        CharacterController.enabled = true;
        CharacterController.SimpleMove(Vector3.zero);

        yield return new WaitForSeconds(time);
        GlitchEffect.enabled = false;
    }

    private void Update()
    {
        SlowmoMode();
        SetTargetIconColor();
        PerformTeleport();

        _highScore = Mathf.Max(PlayerTransform.position.y, _highScore);

        // UI
        JumpsText.text = CurrentJumpsLeft.ToString();
        HighScoreText.text = _highScore.ToString("0.00") + " m";



    }

    public List<Material> StripeMaterials;
    private float blinkInvalidTimer;
    public float BlinkInvalidInterval = 0.2f;
    private void SetTargetIconColor()
    {
        _targeticonMeshRenderer.material = StripeMaterials[CurrentJumpsLeft];

        if (CurrentJumpsLeft <= 0)
        {
            blinkInvalidTimer += Time.deltaTime;
            if (blinkInvalidTimer >= BlinkInvalidInterval)
            {
                blinkInvalidTimer = 0;
                _targeticonMeshRenderer.enabled = !_targeticonMeshRenderer.enabled;
            }
        }
        else
            _targeticonMeshRenderer.enabled = true;
    }

    public float MusicSlowMoPitch = 0.75f;
    private void SlowmoMode()
    {
        if (Input.GetKey(SlowmoKey))
        {
            Time.timeScale = SlowmoTimeScale;
            Music.pitch = MusicSlowMoPitch;
            Vignette.enabled = true;
        }
        else
        {
            Time.timeScale = 1;
            Music.pitch = 1;
            Vignette.enabled = false;
        }
    }

    public Color CurrentJumpsTextActiveColor = new Color(0,0,0,1);
    public Color CurrentJumpTextInactiveColor= new Color(0,0,0,0);
    private bool currentlyFading = false;
    private IEnumerator ShowJumpText(float timeToDisplay)
    {
        currentlyFading = true;
        float _colorTimer = 0f;
        JumpsText.color = CurrentJumpsTextActiveColor;
        while (_colorTimer < 1f)
        {
            _colorTimer += Time.deltaTime / timeToDisplay;
            JumpsText.color = Color.Lerp(CurrentJumpsTextActiveColor, CurrentJumpTextInactiveColor, _colorTimer);

            yield return null;
        }

        JumpsText.color = CurrentJumpTextInactiveColor;
        currentlyFading = false;
    }

    private void PerformTeleport()
    {
        
        RaycastHit hit;
        Ray ray = new Ray(CameraTransform.position, CameraTransform.forward);

        Debug.DrawRay(ray.origin, ray.direction * RaycastDistance, Color.green);
        Physics.Raycast(ray, out hit, Mathf.Infinity, ObjectsToHit);

        if (hit.transform != null)
        {
            TargetIcon.position = hit.point;

            if (CurrentJumpsLeft <= 0)
                return;

            if (Input.GetMouseButtonDown(0))
                CastLine(hit.point);
        }

    }


    public float ShowJumpColorTextTime = 0.7f;
    public void ResetTeleportJumps()
    {
        if (CurrentJumpsLeft != MaxNumberOfJumpsBeforeTouchingGround)
        {
            if (!currentlyFading)
                StartCoroutine(ShowJumpText(ShowJumpColorTextTime));
        }

        CurrentJumpsLeft = MaxNumberOfJumpsBeforeTouchingGround;


    }

    public void PowerUp()
    {
        CurrentJumpsLeft++;
        if (CurrentJumpsLeft > MaxNumberOfJumpsBeforeTouchingGround)
            CurrentJumpsLeft = MaxNumberOfJumpsBeforeTouchingGround;

        if (!currentlyFading)
            StartCoroutine(ShowJumpText(ShowJumpColorTextTime));

    }

    private void CastLine(Vector3 endPosition)
    {
        CurrentJumpsLeft--;

        if (!currentlyFading)
            StartCoroutine(ShowJumpText(ShowJumpColorTextTime));

        StopCoroutine("MoveToPosition");


        LineRenderer.SetPosition(0, PlayerTransform.position);
        LineRenderer.SetPosition(1, TargetIcon.position);

        StartCoroutine(MoveToPosition(TargetIcon.transform.position - (CharacterController.radius*2 * PlayerTransform.forward), TimeToMoveLerp));

    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float timeToMove)
    {
        var currentPos = PlayerTransform.position;

        CurrentlyTeleporting = true;


        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;

            PlayerCamera.fieldOfView += t;
            if (PlayerCamera.fieldOfView > TeleportingFOV)
                PlayerCamera.fieldOfView = TeleportingFOV;


            PlayerTransform.position = Vector3.Lerp(currentPos, targetPosition, t);


            LineRenderer.enabled = true;

            yield return null;
        }
        CurrentlyTeleporting = false;
        LineRenderer.enabled = false;


        StartCoroutine(TempGravityDisabled());
    }

    public float TempGravityTime = 0.6f;
    private IEnumerator TempGravityDisabled()
    {
        CharacterController.enabled = false;

        float t = 0;
        while (t < TempGravityTime)
        {
            t += Time.deltaTime;

           /* PlayerCamera.fieldOfView -= t;
            if (PlayerCamera.fieldOfView < _standardFOV)
                PlayerCamera.fieldOfView = _standardFOV;
                */

            // mid jump
            if (Math.Abs(CrossPlatformInputManager.GetAxis("Vertical")) > 0.1f ||
                Math.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > 0.1f ||
                CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                t = TempGravityTime;
                CancelTeleport();
            }
            yield return null;
        }
        //PlayerCamera.fieldOfView = _standardFOV;

        CharacterController.enabled = true;
    }

    private void CancelTeleport()
    {
        if (CurrentlyTeleporting)
            return;

        //PlayerCamera.fieldOfView = _standardFOV;

        CharacterController.enabled = true;
        FirstPersonController.PerformTeleportJump();
        //cha
    }
}