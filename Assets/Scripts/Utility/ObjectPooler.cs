using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Class used to pool objects for instantiation. Allows alike objects to be deactivated and reused later instead of destroyed and instantiated anew.
/// </summary>
public static class ObjectPooler
{
    /// <summary>
    /// Dictionary of current objects in scene.
    /// </summary>
    private static readonly Dictionary<string, List<GameObject>> currentObjects = new Dictionary<string, List<GameObject>>();
    /// <summary>
    /// Dictionary of inactive objects ready to be reused.
    /// </summary>
    private static readonly Dictionary<string, List<GameObject>> availableObjects = new Dictionary<string, List<GameObject>>();


    /// <summary>
    /// Clears all data on game quit.
    /// </summary>
    public static void Quit()
    {
        foreach(KeyValuePair<string, List<GameObject>> currentObject in currentObjects)
        {
            foreach(GameObject nextObject in currentObject.Value)
            {
                GameObject.Destroy(nextObject);
            }
        }
        currentObjects.Clear();
        foreach(KeyValuePair<string, List<GameObject>> availableObject in currentObjects)
        {
            foreach(GameObject nextObject in availableObject.Value)
            {
                GameObject.Destroy(nextObject);
            }
        }
        availableObjects.Clear();
    }

    /// <summary>
    /// Instantiates a game object. If there is an inactive version of this object it will just reactivate it and move it to the new location.
    /// If there is are no inactive versions of this object, it will instead instantiate a new object.
    /// </summary>
    /// <param name="gameObject">The object to instantiate.</param>
    /// <param name="position">The position to place the object.</param>
    /// <param name="rotation">The rotation to give the object.</param>
    /// <returns>Returns a reference to the instantiated object.</returns>
    public static GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        string objectName = gameObject.name;
        // If Available Objects does not contain a key for this object type, add that key and an empty list for value
        if(availableObjects.ContainsKey(objectName) == false)
        {
            availableObjects.Add(objectName, new List<GameObject>());
        }
        // If Current Objects does not contain a key for this object type, add that key and an empty dictionary
        if(currentObjects.ContainsKey(objectName) == false)
        {
            currentObjects.Add(objectName, new List<GameObject>());
        }
        // If the Available Objects value list has entries and the sum of it and current objects entries are more than min pooled objects
        if(availableObjects[objectName].Count > 0 && availableObjects[objectName].Count + currentObjects[objectName].Count > GameManager.Instance.MinPooledObjects)
        {
            GameObject go = availableObjects[objectName][0];
            availableObjects[objectName].Remove(go);
            currentObjects[objectName].Add(go);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.SetActive(true);
            return go;
        }
        // If there are no entries in Available Objects list or there is less than min pooled objects
        else
        {
            GameObject go = Object.Instantiate(gameObject, position, rotation);
            go.name = objectName;
            currentObjects[objectName].Add(go);
            go.SetActive(true);
            return go;
        }
    }

    /// <summary>
    /// Instantiates a game object. If there is an inactive version of this object it will just reactivate it and move it to the new location.
    /// If there is are no inactive versions of this object, it will instead instantiate a new object.
    /// </summary>
    /// <param name="gameObject">The object to instantiate.</param>
    /// <param name="position">The position to place the object.</param>
    /// <param name="rotation">The rotation to give the object.</param>
    /// <param name="parent">The parent to make this game object child of.</param>
    /// <returns>Returns a reference to the instantiated object.</returns>
    public static GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        string objectName = gameObject.name;
        // If Available Objects does not contain a key for this object type, add that key and an empty list for value
        if(availableObjects.ContainsKey(objectName) == false)
        {
            availableObjects.Add(objectName, new List<GameObject>());
        }
        // If Current Objects does not contain a key for this object type, add that key and an empty dictionary
        if(currentObjects.ContainsKey(objectName) == false)
        {
            currentObjects.Add(objectName, new List<GameObject>());
        }
        // If the Available Objects value list has entries
        if(availableObjects[objectName].Count > 0 && availableObjects[objectName].Count + currentObjects[objectName].Count > GameManager.Instance.MinPooledObjects)
        {
            GameObject go = availableObjects[objectName][0];
            availableObjects[objectName].Remove(go);
            currentObjects[objectName].Add(go);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.parent = parent;
            go.transform.localScale = new Vector3(1 / parent.localScale.x, 1 / parent.localScale.y, 1 / parent.localScale.z);
            go.SetActive(true);
            return go;
        }
        // If there are no entries in Available Objects list
        else
        {
            GameObject go = Object.Instantiate(gameObject, position, rotation);
            go.name = objectName;
            currentObjects[objectName].Add(go);
            go.transform.parent = parent;
            go.transform.localScale = new Vector3(1 / parent.localScale.x, 1 / parent.localScale.y, 1 / parent.localScale.z);
            go.SetActive(true);
            return go;
        }
    }

    /// <summary>
    /// Deactivates a game object to be reused later.
    /// </summary>
    /// <param name="gameObject">The game object to deactivate.</param>
    public static void Destroy(GameObject gameObject)
    {
        if(gameObject.activeSelf == true)
        {
            string objectName = gameObject.name;
            gameObject.SetActive(false);
            currentObjects[objectName].Remove(gameObject);
            availableObjects[objectName].Add(gameObject);
        }
    }

    /// <summary>
    /// Deactivates a game object after a period of time to be reused later.
    /// </summary>
    /// <param name="gameObject">The game object to deactivate.</param>
    /// <param name="seconds">The number of seconds to wait for before deactivating object.</param>
    public static void Destroy(GameObject gameObject, float seconds)
    {
        StaticCoroutine.StartCR(DestroyAfterTime(gameObject, seconds));
    }

    /// <summary>
    /// Coroutine for deactivating a game object after an amount of time.
    /// </summary>
    /// <param name="gameObject">The game object to deactivate.</param>
    /// <param name="seconds">The number of seconds to wait for before deactivating object.</param>
    /// <returns>Returns IEnumerator for coroutine.</returns>
    private static IEnumerator DestroyAfterTime(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
