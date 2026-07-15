using UnityEngine;

public static class SceneObjectLookup
{
    public static GameObject FindGameObject(string objectName)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Transform candidate in transforms)
        {
            if (candidate.gameObject.scene.IsValid() && candidate.name == objectName)
                return candidate.gameObject;
        }

        return null;
    }

    public static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject target = FindGameObject(objectName);
        return target == null ? null : target.GetComponent<T>();
    }
}
