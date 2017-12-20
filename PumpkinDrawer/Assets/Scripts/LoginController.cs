using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JI.Unity.PumpkinCarver.Drawer
{
    public class LoginController : MonoBehaviour
    {
        public InputField addressInputField;

        public void Login()
        {
            GameState.HostAddress = addressInputField.text;
            SceneManager.LoadScene("drawer");
        }
    }
}