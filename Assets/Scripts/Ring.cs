using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private Objective objectiveScript;
    private bool ringActive = false;

    private void Start()
    {
        objectiveScript = FindObjectOfType<Objective>();
    }

    public void ActivateRing()
    {
        ringActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // si el anillo esta activo
        // Dile al scrip objetivo que se ha pasado
        if (ringActive)
        {
            objectiveScript.NextRing();
            Destroy(gameObject, 5.0f);
        }

    }
}
