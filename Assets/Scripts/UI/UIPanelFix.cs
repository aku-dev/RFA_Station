using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIPanelFix : MonoBehaviour
{
    [SerializeField] private Selectable FirstSelect = null;

    private bool runSelect = false;
    private void OnEnable()
    {
        runSelect = true;
    }

    private void LateUpdate()
    {
        if(runSelect)
        {
            EventSystem.current.SetSelectedGameObject(FirstSelect.gameObject);
            FirstSelect.Select();
            runSelect = false;
        }
    }
}
