using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "menu_script", menuName = "Scriptable Objects/menu_script")]
public class menu_script : ScriptableObject
{
    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
