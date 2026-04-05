using UnityEngine;

namespace HollowManor
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeBootstrap()
        {
#if UNITY_2023_1_OR_NEWER
            if (Object.FindFirstObjectByType<BootstrapController>() != null)
#else
            if (Object.FindObjectOfType<BootstrapController>() != null)
#endif
            {
                return;
            }

            GameObject bootstrapObject = new GameObject("_DeathForestBootstrap");
            bootstrapObject.AddComponent<BootstrapController>();
        }
    }
}
