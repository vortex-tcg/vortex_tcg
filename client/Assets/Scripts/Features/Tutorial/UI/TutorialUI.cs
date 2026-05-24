using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string backButtonName = "BackButton";
    [SerializeField] private string tutorialVideoElementName = "TutorialVideo";
    [SerializeField] private string tutorialVideoResourcePath = "Video/tuto-vortex";
<<<<<<< HEAD
    [SerializeField] private string tutorialVideoUrl = "";
=======
>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
    [SerializeField] private string tutorialRenderTextureResourcePath = "Video/TutorialRT";
    [SerializeField] private string homeSceneName = "HomeScene";

    private Button backButton;
    private VisualElement tutorialVideoElement;
    private RenderTexture tutorialRenderTexture;
    private VideoPlayer tutorialVideoPlayer;
    private GameObject tutorialVideoPlayerObject;
    private bool isTutorialVideoPaused;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[TutorialUI] UIDocument missing.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[TutorialUI] rootVisualElement missing.");
            return;
        }

        tutorialVideoElement = root.Q<VisualElement>(tutorialVideoElementName);
        backButton = root.Q<Button>(backButtonName);

        if (tutorialVideoElement == null)
        {
            Debug.LogWarning($"[TutorialUI] VisualElement '{tutorialVideoElementName}' not found in UXML.");
        }
        else
        {
            tutorialVideoElement.pickingMode = PickingMode.Position;
            tutorialVideoElement.RegisterCallback<ClickEvent>(OnTutorialVideoClicked);

            tutorialRenderTexture ??= Resources.Load<RenderTexture>(tutorialRenderTextureResourcePath);
            if (tutorialRenderTexture != null)
            {
                tutorialVideoElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tutorialRenderTexture));
                StartTutorialVideo();
            }
            else
            {
                Debug.LogWarning($"[TutorialUI] RenderTexture introuvable dans Resources : {tutorialRenderTextureResourcePath}");
            }
        }

        if (backButton == null)
        {
            Debug.LogWarning($"[TutorialUI] Button '{backButtonName}' not found in UXML.");
            return;
        }

        backButton.clicked += OnBackClicked;
    }

    private void OnDisable()
    {
        if (tutorialVideoElement != null)
            tutorialVideoElement.UnregisterCallback<ClickEvent>(OnTutorialVideoClicked);

        if (backButton != null)
            backButton.clicked -= OnBackClicked;

        StopTutorialVideo();
    }

    private void StartTutorialVideo()
    {
        if (tutorialVideoPlayer != null)
            return;
<<<<<<< HEAD
        VideoClip clip = null;
        bool useUrl = !string.IsNullOrWhiteSpace(tutorialVideoUrl);
        if (!useUrl)
        {
            clip = Resources.Load<VideoClip>(tutorialVideoResourcePath);
            if (clip == null)
            {
                Debug.LogWarning($"[TutorialUI] VideoClip introuvable dans Resources : {tutorialVideoResourcePath} (ou tutorialVideoUrl non fourni)");
                // continue, if URL also empty we'll bail later
            }
        }
        Debug.Log("[TutorialUI] StartTutorialVideo: creating VideoPlayer (useUrl=" + useUrl + ")");
=======

        VideoClip clip = Resources.Load<VideoClip>(tutorialVideoResourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[TutorialUI] VideoClip introuvable dans Resources : {tutorialVideoResourcePath}");
            return;
        }

>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
        tutorialVideoPlayerObject = new GameObject("TutorialVideoPlayer");
        tutorialVideoPlayerObject.transform.SetParent(transform, false);
        tutorialVideoPlayer = tutorialVideoPlayerObject.AddComponent<VideoPlayer>();
        tutorialVideoPlayer.playOnAwake = false;
        tutorialVideoPlayer.isLooping = true;
        tutorialVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        tutorialVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
<<<<<<< HEAD
        // Ensure RenderTexture is created/allocated
        if (tutorialRenderTexture != null)
        {
            try
            {
                if (!tutorialRenderTexture.IsCreated())
                {
                    Debug.Log("[TutorialUI] RenderTexture not created yet - calling Create()");
                    tutorialRenderTexture.Create();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TutorialUI] Exception while creating RenderTexture: {ex.Message}");
            }

            tutorialVideoPlayer.targetTexture = tutorialRenderTexture;
            Debug.Log($"[TutorialUI] Assigned targetTexture (w={tutorialRenderTexture.width}, h={tutorialRenderTexture.height})");
        }
        else
        {
            Debug.LogWarning("[TutorialUI] tutorialRenderTexture is null when assigning to VideoPlayer.");
        }

        // Register events to help debug playback
        tutorialVideoPlayer.errorReceived += OnTutorialVideoError;
        tutorialVideoPlayer.started += OnTutorialVideoStarted;
        tutorialVideoPlayer.loopPointReached += OnTutorialVideoLoopPointReached;

        if (useUrl)
        {
            tutorialVideoPlayer.source = VideoSource.Url;
            tutorialVideoPlayer.url = tutorialVideoUrl;
        }
        else if (clip != null)
        {
            tutorialVideoPlayer.source = VideoSource.VideoClip;
            tutorialVideoPlayer.clip = clip;
        }
        else
        {
            Debug.LogWarning("[TutorialUI] Aucun clip disponible (ni URL ni Resource). Video non lancée.");
            Destroy(tutorialVideoPlayer);
            tutorialVideoPlayer = null;
            return;
        }

        tutorialVideoPlayer.prepareCompleted += OnTutorialVideoPrepared;
        Debug.Log("[TutorialUI] Calling Prepare() on VideoPlayer...");
=======
        tutorialVideoPlayer.targetTexture = tutorialRenderTexture;
        tutorialVideoPlayer.clip = clip;
        tutorialVideoPlayer.prepareCompleted += OnTutorialVideoPrepared;
>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
        tutorialVideoPlayer.Prepare();
    }

    private void OnTutorialVideoPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnTutorialVideoPrepared;
<<<<<<< HEAD
        Debug.Log("[TutorialUI] Video prepared. isPrepared=" + source.isPrepared + " length=" + source.length);
=======
>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
        isTutorialVideoPaused = false;
        source.Play();
    }

<<<<<<< HEAD
    private void OnTutorialVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[TutorialUI] VideoPlayer error: {message}");
    }

    private void OnTutorialVideoStarted(VideoPlayer source)
    {
        Debug.Log("[TutorialUI] VideoPlayer started. isPlaying=" + source.isPlaying);
    }

    private void OnTutorialVideoLoopPointReached(VideoPlayer source)
    {
        Debug.Log("[TutorialUI] VideoPlayer loopPointReached");
    }

=======
>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
    private void OnTutorialVideoClicked(ClickEvent evt)
    {
        if (tutorialVideoPlayer == null)
            return;

        isTutorialVideoPaused = !isTutorialVideoPaused;

        if (isTutorialVideoPaused)
            tutorialVideoPlayer.Pause();
        else
            tutorialVideoPlayer.Play();

        evt.StopPropagation();
    }

    private void StopTutorialVideo()
    {
        if (tutorialVideoPlayer != null)
        {
            tutorialVideoPlayer.prepareCompleted -= OnTutorialVideoPrepared;
<<<<<<< HEAD
            tutorialVideoPlayer.errorReceived -= OnTutorialVideoError;
            tutorialVideoPlayer.started -= OnTutorialVideoStarted;
            tutorialVideoPlayer.loopPointReached -= OnTutorialVideoLoopPointReached;
=======
>>>>>>> 43d59c5 ([FEAT] Tutorial added in game)
            tutorialVideoPlayer.Stop();
            Destroy(tutorialVideoPlayer);
            tutorialVideoPlayer = null;
        }

        if (tutorialVideoPlayerObject != null)
        {
            Destroy(tutorialVideoPlayerObject);
            tutorialVideoPlayerObject = null;
        }

        isTutorialVideoPaused = false;
    }

    private void OnBackClicked()
    {
        LoadingScreen.Load(homeSceneName, loadMenu: true, unloadMenu: false);
    }
}
