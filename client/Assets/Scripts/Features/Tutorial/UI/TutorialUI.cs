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

        VideoClip clip = Resources.Load<VideoClip>(tutorialVideoResourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[TutorialUI] VideoClip introuvable dans Resources : {tutorialVideoResourcePath}");
            return;
        }

        tutorialVideoPlayerObject = new GameObject("TutorialVideoPlayer");
        tutorialVideoPlayerObject.transform.SetParent(transform, false);
        tutorialVideoPlayer = tutorialVideoPlayerObject.AddComponent<VideoPlayer>();
        tutorialVideoPlayer.playOnAwake = false;
        tutorialVideoPlayer.isLooping = true;
        tutorialVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        tutorialVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        tutorialVideoPlayer.targetTexture = tutorialRenderTexture;
        tutorialVideoPlayer.clip = clip;
        tutorialVideoPlayer.prepareCompleted += OnTutorialVideoPrepared;
        tutorialVideoPlayer.Prepare();
    }

    private void OnTutorialVideoPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnTutorialVideoPrepared;
        isTutorialVideoPaused = false;
        source.Play();
    }

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
