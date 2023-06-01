using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using TMPro;

public class spawnball : MonoBehaviour
{
    public GameObject spherespawn;
    public GameObject arrow;
    public GameObject text;
    public TextMeshProUGUI numberText;
    int counter;

    public void ButtonPressed()
    {
        counter++;
        numberText.text = counter + "";
        spherespawn = Instantiate(spherespawn, transform.position, Quaternion.identity);
        Destroy(arrow);
        Destroy(text);
        if (spherespawn != null && spherespawn.transform.position.y <= -2000)
        {
            Destroy(spherespawn);
        }
    }
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            

        }
    }
}
