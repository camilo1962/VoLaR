using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { set; get; }

    public Material playerMaterial;
    public Color[] playerColors = new Color[10];
    public GameObject[] playerTrails = new GameObject[10];

    public int currentLevel = 0;    // Paraq cambiar en el menu la escena de juego
    public int menuFocus = 0;     //pasar de la escena de juego al menu

    private Dictionary<int, Vector2> activeTouches = new Dictionary<int, Vector2>();

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    

    public Vector3 GetPlayerInput()
    {
        //puede utilizar aelerometro?
        if (SaveManager.Instance.state.usingAccelerometer)
        {
            //Si no lo puede utilizar, cambia Y y Z, no necesitamos eso Y
            Vector3 a = Input.acceleration;
            a.y = a.z;
            return a;
        }

        //Leer todos los toques del usuario
        Vector3 r = Vector3.zero;
        foreach(Touch touch in Input.touches)
        {
            // Si empezáramos a presionar en la pantalla
            if(touch.phase == TouchPhase.Began)
            {
                activeTouches.Add(touch.fingerId, touch.position);
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if (activeTouches.ContainsKey(touch.fingerId))
                    activeTouches.Remove(touch.fingerId);
            }
            else
            {
                float mag = 0;
                r = (touch.position - activeTouches[touch.fingerId]);
                mag = r.magnitude / 300;
                r = r.normalized * mag;

                
            }

        }
        return r;
    }
}
