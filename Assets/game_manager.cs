using UnityEngine;

[CreateAssetMenu(fileName = "game_manager", menuName = "Scriptable Objects/game_manager")]
public class game_manager : ScriptableObject
{
    public enum Animals
    {
        Shark,
        Fish,
        Shrimp
    }
}
