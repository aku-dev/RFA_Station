using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIVersion : MonoBehaviour
{
    private void OnEnable()
    {        
        Text txt = GetComponent<Text>();
        if(txt != null)
        {
            txt.text = $"© AK Studio 2019-2023 Version: {Application.version} ";
        }
    }
}
