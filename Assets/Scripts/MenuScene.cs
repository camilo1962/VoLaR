using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    private float fadeInSpeed = 0.33f;

    public RectTransform menuContainer;
    public Transform levelPanel;
    public Transform colorPanel;
    public Transform trailPanel;

    public Button tiltControlButton;
    public Color tiltControlEnabled ;
    public Color tiltControlDisabled;


    public Text colorBuySetText;
    public Text trailBuySetText;
    public Text goldText;

    private MenuCamera menuCam; 

    private int[] colorCost = new int[] { 0, 5, 5, 5, 10, 10, 10, 15, 15, 10 };
    private int[] trailCost = new int[] { 0, 20, 40, 40, 60, 60, 80, 80, 100, 100 };
    private int selectedColorIndex;
    private int selectedTrailIndex;
    private int activeColorIndex;
    private int activeTrailIndex;

    private Vector3 desiredMenuPosition;

    private GameObject currentTrail;

    public AnimationCurve enteringLevelZoomCurve;
    private bool isEnteringLevel = false;
    private float zoomDuration = 3.0f;
    private float zoomTransition;

    private Texture previousTrail;
    private GameObject lastPreviewObject;

    public Transform trailPreviewObject;
    public RenderTexture trailPreviewTexture;

    private void Start()
    {
       

        //comprobar si tenemos un acelerómetro
        if (SystemInfo.supportsAccelerometer)
        {
            tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;
        }
        else
        {
            tiltControlButton.gameObject.SetActive(false);
        }

        menuCam = FindObjectOfType<MenuCamera>();


        //Posicion de la camara al enfocar al menu
        SetCameraTo(Manager.Instance.menuFocus);

        //Tell our
        UpdateGoldText();

        //Toma el único CanvasGroup en la escena
        fadeGroup = FindObjectOfType<CanvasGroup>();
        //Comience con una pantalla blanca
        fadeGroup.alpha = 1;

        // eventos delos botones compras
        InitShop();
        // Eventos de los botones niveles
        IniLevel();

        //establecer preferencias de jugador (color y rastro
        OnColorSelect(SaveManager.Instance.state.activeColor);
        SetColor(SaveManager.Instance.state.activeColor);

        OnTrailSelect(SaveManager.Instance.state.activeTrail);
        SetTrail(SaveManager.Instance.state.activeTrail);

        //Hacer los botones más grandes para los seleccionados
        colorPanel.GetChild(SaveManager.Instance.state.activeColor).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;
        trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        // Crear ruta previa
        lastPreviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]) as GameObject;
        lastPreviewObject.transform.SetParent(trailPreviewObject);
        lastPreviewObject.transform.localPosition = Vector3.zero;
    }
    private void Update()
    {
        fadeGroup.alpha = 1 - Time.timeSinceLevelLoad * fadeInSpeed;

        menuContainer.anchoredPosition3D = Vector3.Lerp(menuContainer.anchoredPosition3D, desiredMenuPosition, 0.1f);

        //Entering level
        if (isEnteringLevel)
        {
            zoomTransition += (1 / zoomDuration) * Time.deltaTime;

            menuContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5, enteringLevelZoomCurve.Evaluate(zoomTransition));

            Vector3 newDesiredPosition = desiredMenuPosition * 5;

            RectTransform rt = levelPanel.GetChild(Manager.Instance.currentLevel).GetComponent<RectTransform>();
            newDesiredPosition -= rt.anchoredPosition3D * 5;


            menuContainer.anchoredPosition3D = Vector3.Lerp(desiredMenuPosition, newDesiredPosition, enteringLevelZoomCurve.Evaluate(zoomTransition));

            fadeGroup.alpha = zoomTransition;

            if(zoomTransition >= 1)
            {
                SceneManager.LoadScene("Game");
            }
        }
    }

    private void InitShop()
    {
        if (colorPanel == null || trailPanel == null)
            Debug.Log("No tienes asignado color/ruta en el inspector");

        // transformar nuestro panel de color
        int i = 0;
        foreach(Transform t in colorPanel)
        {
            int currentIndex = i;

            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnColorSelect(currentIndex));

            //Establecer el color de la imagen, en función de si es propiedad o no
            Image img = t.GetComponent<Image>();
            img.color = SaveManager.Instance.IsColorOwned(i)
                ? Manager.Instance.playerColors[currentIndex]
                : Color.Lerp(Manager.Instance.playerColors[currentIndex], new Color(0,0,0,1), 0.25f); 

            i++;
        }

        // Reset index
        i = 0;
        foreach (Transform t in trailPanel)
        {
            int currentIndex = i;

            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnTrailSelect(currentIndex));

            //Establecer el color de la imagen, en función de si es propiedad o no
            RawImage img = t.GetComponent<RawImage>();
            img.color = SaveManager.Instance.IsTrailOwned(i) ? Color.white : new Color(0.7f, 0.7f, 0,7f);

            i++;
        }

        //establecer el camino anterior, para evitar errores al cambiar más tarde
        previousTrail = trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RawImage>().texture;

    }

    private void IniLevel()
    {
        if (levelPanel == null)
            Debug.Log("No tienes asignado un nivel en el inspector");

        // transformar nuestro panel de color
        int i = 0;
        foreach (Transform t in levelPanel)
        {
            int currentIndex = i;

            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnLevelSelect(currentIndex));

            Image img = t.GetComponent<Image>();

            if(i <= SaveManager.Instance.state.completedLevel)
            {
                if(i == SaveManager.Instance.state.completedLevel)
                {
                    img.color = Color.white;
                }
                else
                {
                    img.color = Color.green;
                }
            }
            else
            {
                b.interactable = false;
                img.color = Color.grey;
            }

            i++;
        }
    }

    private void SetCameraTo(int menuIndex)
    {
        NavigateTo(menuIndex);
        menuContainer.anchoredPosition3D = desiredMenuPosition;
    }

    private void NavigateTo(int menuIndex)
    {
        switch (menuIndex)
        {
            default:
            case 0:
                desiredMenuPosition = Vector3.zero;
                menuCam.BackToMainMenu();
                break;
            case 1:
                desiredMenuPosition = Vector3.right * 1334;
                menuCam.MoveToLevel();
                break;
            case 2:
                desiredMenuPosition = Vector3.left * 1334;
                menuCam.MoveToShop();
                break;
        }
    }

    private void SetColor(int index)
    {
        // Set the active index
        activeColorIndex = index;
        SaveManager.Instance.state.activeColor = index;

        //Cambiar el color del jugador

        Manager.Instance.playerMaterial.color = Manager.Instance.playerColors[index];



        // Cambiar boton compra ajustes
        colorBuySetText.text = "Actualiza";

        SaveManager.Instance.Save();
    }

    private void SetTrail(int index)
    {
        //Set active index
        activeTrailIndex = index;
        SaveManager.Instance.state.activeTrail = index;

        //Cambiar la ruta del jugador
        if (currentTrail != null)
            Destroy(currentTrail);
        //Crear la nueva ruta
        currentTrail = Instantiate(Manager.Instance.playerTrails[index]) as GameObject;

        //se configura como hijo del jugador
        //currentTrail.transform.SetParent(FindObjectOfType<MenuPlayer>().transform);
        currentTrail.transform.SetParent(GameObject.FindGameObjectWithTag("Player").transform);
        //solucionar los problemas de escalado extraños/problemas de rotación
        currentTrail.transform.localPosition = Vector3.zero;
        currentTrail.transform.localRotation = Quaternion.Euler(0, 0, 90);
        currentTrail.transform.localScale = Vector3.one * 0.01f;


        // Cambiar boton compra ajustes
        trailBuySetText.text = "Actualiza";

        SaveManager.Instance.Save();

    }

    private void UpdateGoldText()
    {
        goldText.text = SaveManager.Instance.state.gold.ToString();
    }

   // Botones
    public void OnPlayClick()
    {
        NavigateTo(1);
        Debug.Log("Tocaste boton play");
    }

    public void OnShopClick()
    {
        NavigateTo(2);
        Debug.Log("Tocaste boton Stop");
    }

    public void OnBackClick()
    {
        NavigateTo(0);
        Debug.Log("Clica para salir");
    }

    private void OnColorSelect(int currentIndex)
    {
        Debug.Log("Selecciona boton color : " + currentIndex);

        if (selectedColorIndex == currentIndex)
            return;

        colorPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;
        colorPanel.GetChild(selectedColorIndex).GetComponent<RectTransform>().localScale = Vector3.one;


        selectedColorIndex = currentIndex;

        if (SaveManager.Instance.IsColorOwned(currentIndex))
        {
            if(activeColorIndex == currentIndex)
            {
                colorBuySetText.text = "Actializa";
            }
            else
            {
                colorBuySetText.text = "Selecciona";
            }
            
        }
        else
        {
            colorBuySetText.text = "Compra: " + colorCost[currentIndex].ToString();
        }
    }

    private void OnTrailSelect(int currentIndex)
    {
        Debug.Log("Selecciona boton ruta : " + currentIndex);

        if (selectedTrailIndex == currentIndex)
            return;

        //Ruta previa
        //obtener la imagen del botón de vista previa
        trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().texture = previousTrail;
        //mantener la imagen de vista previa del nuevo sendero en el sendero anterior
        previousTrail = trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture;
        //establecer la nueva imagen de vista previa de la ruta en la otra cámara
        trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture = trailPreviewTexture;

        //Cambia el objjeto fisico por la ruta previa
        if(lastPreviewObject != null)
             Destroy(lastPreviewObject);
        lastPreviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[currentIndex]) as GameObject;
        lastPreviewObject.transform.SetParent(trailPreviewObject);
        lastPreviewObject.transform.localPosition = Vector3.zero;

        trailPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;
        trailPanel.GetChild(selectedTrailIndex).GetComponent<RectTransform>().localScale = Vector3.one;

        selectedTrailIndex = currentIndex;

        if (SaveManager.Instance.IsTrailOwned(currentIndex))
        {
            if (activeTrailIndex == currentIndex)
            {
                trailBuySetText.text = "Actualiza";
            }
            else
            {
                trailBuySetText.text = "Selecciona";
            }
        }
        else
        {
            trailBuySetText.text = "Compra: " + trailCost[currentIndex].ToString();
        }
    }

    private void OnLevelSelect(int currentIndex)
    {
        Manager.Instance.currentLevel = currentIndex;
        isEnteringLevel = true;
        Debug.Log("Selecciona nivel: " + currentIndex);
    }

    public void OnColorBuySet()
    {
        Debug.Log("Compra/ajusta color");

        if (SaveManager.Instance.IsColorOwned(selectedColorIndex))
        {
            SetColor(selectedColorIndex);
        }
        else
        {
            if (SaveManager.Instance.BuyColor(selectedColorIndex, colorCost[selectedColorIndex]))
            {
                SetColor(selectedColorIndex);

                colorPanel.GetChild(selectedColorIndex).GetComponent<Image>().color = Manager.Instance.playerColors[selectedColorIndex];
                


                UpdateGoldText();
            }
            else
            {
                Debug.Log("Insuficiente Oro");
            }
        }
    }

    public void OnTrailButSet()
    {
        Debug.Log("Compra/ajusta ruta");

        if (SaveManager.Instance.IsTrailOwned(selectedTrailIndex))
        {
            SetTrail(selectedTrailIndex);
        }
        else
        {
            if (SaveManager.Instance.BuyTrail(selectedTrailIndex, trailCost[selectedTrailIndex]))
            {
                SetTrail(selectedTrailIndex);

                trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().color = Color.white;

                UpdateGoldText();
            }
            else
            {
                Debug.Log("Insuficiente Oro");
            }
        }
    }

    public void OnTiltControl()
    {
        SaveManager.Instance.state.usingAccelerometer = !SaveManager.Instance.state.usingAccelerometer;

        SaveManager.Instance.Save();

        tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;
    }
}
