using System;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
[SerializeField] Chicken playerChicken;
[SerializeField] private TextMeshProUGUI textDisplay;

private void OnEnable()
{
   playerChicken.onInteractibleObjectChanged += updText;
   updText();
}

private void updText()
{ 
    bool InteractibleExist =  playerChicken.InteractibleObj;
    textDisplay.enabled = InteractibleExist;
    if (InteractibleExist) 
        textDisplay.text = "      E    Interact\n      " + playerChicken.InteractibleObj.name;
}

private void OnDisable()
{
    playerChicken.onInteractibleObjectChanged -= updText;
    
}
}
