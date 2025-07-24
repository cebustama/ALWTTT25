using ALWTTT.Managers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.Utils
{
    public class CoreLoader : MonoBehaviour
    {
        private void Awake()
        {
            try
            {
                if (!GameManager.Instance)
                {
                    SceneManager.LoadScene("ALWTTTCore", LoadSceneMode.Additive);
                }
                Destroy(gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError("You should add ALWTTTCore scene to build settings!");
                throw;
            }
        }
    }
}