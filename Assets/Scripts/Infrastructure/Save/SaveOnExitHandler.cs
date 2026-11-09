using UnityEngine;
using VContainer;

public sealed class SaveOnExitHandler : MonoBehaviour
{
    private ISaveService _saveService;

    [Inject]
    public void Construct(ISaveService saveService)
    {
        _saveService = saveService;
    }

    private void OnApplicationQuit()
    {
        Save("quit");
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            Save("pause");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            Save("focus lost");
    }

    private void Save(string reason)
    {
        if (_saveService == null)
        {
            Log.Error($"[SaveOnExit] Save skipped ({reason}): ISaveService is not injected");
            return;
        }

        Log.Info($"[SaveOnExit] Save before {reason}");
        _saveService.Save();
    }
}
