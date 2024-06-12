using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform uiElement;
    [SerializeField] private Image mineralHP;
    [SerializeField] private Vector3 offset;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        SetUIActive(false);
    }

    void Update()
    {
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position + offset);
        uiElement.position = screenPosition;
    }

    public void SetUIActive(bool active)
    {
        this.gameObject.SetActive(active);
    }

    public void SetMineralHP(float max, float now)
    {
        mineralHP.fillAmount = (now / max);
    }
}
