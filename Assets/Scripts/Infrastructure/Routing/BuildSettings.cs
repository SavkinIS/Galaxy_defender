using System;

public static class BuildSettings
{
    public static string GetSceneName(SceneId sceneId)
    {
        return sceneId switch
        {
            SceneId.Boot => "Boot",
            SceneId.Lobby => "Lobby",
            SceneId.Game => "Game",
            _ => throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, null)
        };
    }
}