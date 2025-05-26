using UnityEngine;

public class IconStarter : MonoBehaviour
{
    public Sprite goldIcon;
    public Sprite gemIcon;
    public Sprite elixirIcon;

    void Awake()
    {
        CurrencyIconManager.Initialize(goldIcon, gemIcon);
    }
}