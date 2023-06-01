using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class winnertrigger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Touch");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}