using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { set; get; }
    public SaveState state;

    private void Awake()
    {
        //ResetSave();
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Load();

          
        // Estamos acelorómetro y podemos usarlo
        if(state.usingAccelerometer && !SystemInfo.supportsAccelerometer)
        {
            //si no podemos, asegúrese de que no lo intentemos la próxima vez
            state.usingAccelerometer = false;
            Save();
        }

    }

    public void Save()
    {
        PlayerPrefs.SetString("save", Helper.Encrypt( Helper.Serialize<SaveState>(state)));
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("save"))
        {
            Debug.Log(PlayerPrefs.GetString("save"));
            state = Helper.Deserialize<SaveState>(Helper.Decrypt( PlayerPrefs.GetString("save")));
        }
        else
        {
            state = new SaveState();
            Save();
            Debug.Log("No se ha guardado el archivo");
        }
    }

    //Comprueba el color está conseguido
    public bool IsColorOwned(int index)
    {
        return (state.colorOwned & (1 << index)) != 0;
    }

    //Comprueba la ruta está conseguida
    public bool IsTrailOwned(int index)
    {
        return (state.trailOwned & (1 << index)) != 0;
    }

    //Intente comprar un color, devuelva verdadero/falso
    public bool BuyTrail(int index, int cost)
    {
        if(state.gold >= cost)
        {
            state.gold -= cost;
            UnLockTrail(index);

            Save();

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool BuyColor(int index, int cost)
    {
        if (state.gold >= cost)
        {
            state.gold -= cost;
            UnLockColor(index);

            Save();

            return true;
        }
        else
        {
            return false;
        }
    }

    //Desbloquea un color int
    public void UnLockColor(int index)
    {
        state.colorOwned |= 1 << index;
    }
    //Desbloquea una ruta int
    public void UnLockTrail(int index)
    {
        state.trailOwned |= 1 << index;
    }

    public void CompleteLevel(int index)
    {
        if(state.completedLevel == index)
        {
            state.completedLevel++;
            Save();
        }
    }

    // Restablecer todo el archivo guardado
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey("save");
    }
}
