using UnityEngine;

public static class Extensions
{
    public static T GetComponent<T>(this MonoBehaviour mono) where T : Component
    {
        return mono.transform.GetComponent(typeof(T)).As<T>();
    }
    public static T As<T>(this object o)
    {
        return (T)o;
    }
}
