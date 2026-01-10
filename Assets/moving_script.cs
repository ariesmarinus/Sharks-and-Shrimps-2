using UnityEngine;
using UnityEngine.UIElements;

public class moving_script : MonoBehaviour
{
    

    public grid_manager grid_manager;
    public int x;
    public int y;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        Debug.Assert(grid_manager != null);
        Debug.Log("Grid Manager: " + grid_manager.ToString());
        grid_manager.OnClick(x, y);
        
    }
}
