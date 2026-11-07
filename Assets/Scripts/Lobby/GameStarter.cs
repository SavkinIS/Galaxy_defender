using Galaxy.Infrastructure.Routing;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private Button _changeBtn;
    [SerializeField] private SceneId _sceneToLoad;
    private IGlobalRouter _globalRouter;

    private void OnEnable()
    {
        _changeBtn.onClick.RemoveAllListeners();
        _changeBtn.onClick.AddListener(ChangeScene);
    }

    private void OnDisable()
    {
        _changeBtn.onClick.RemoveAllListeners();
       
    }

    [Inject]
    public void Construct(IGlobalRouter globalRouter)
    {
        _globalRouter = globalRouter;
    } 
    
    private void ChangeScene()
    {
        _changeBtn.interactable = false;
        _globalRouter.ChangeScene(_sceneToLoad);
    }
}