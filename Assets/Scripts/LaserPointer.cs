using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VarjoExample;
using Telexistence;
using System.Collections.Generic;

public class LaserPointer : MonoBehaviour
{
    public float maxDistance = 5f; // Maximum distance of the laser pointer
    private LineRenderer lineRenderer;

    public Material originalMaterial;
    public List<Button> btn = new List<Button>();
    public Button btn_NEXT;
    public Controller controller;
    public TLXQuestionnaire tLX;
    private bool hasTriggeredNextQuestion = false;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Always draw the laser as far as maxDistance by default.
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);

        if (controller.primary2DAxisTouch)
        {
            // Regular physics raycast
            RaycastHit hit;
            bool didHit = Physics.Raycast(transform.position, transform.forward, out hit, maxDistance);

            if (didHit && hit.collider.gameObject.CompareTag("btn_photo"))
            {
                // If the raycast hit the photo button, set the end of the laser to the point where the raycast hit and change the button's color.
                lineRenderer.SetPosition(1, hit.point);

                Button button = hit.collider.gameObject.GetComponent<Button>();
                ColorBlock colorBlock = button.colors;
                colorBlock.normalColor = new Color(0,190,255);
                button.colors = colorBlock;

                if (controller.primary2DAxisClick)
                {
                    if (didHit && hit.collider.gameObject.CompareTag("btn_photo") && hit.collider.gameObject.name == "NEXT")
                    {
                        // Only trigger the NextQuestion() method if it hasn't been triggered before
                        if (!hasTriggeredNextQuestion)
                        {
                            tLX.NextQuestion();
                            hasTriggeredNextQuestion = true;
                        }
                    }

                    // Before setting the current button's isclicked property to true,
                    // check the other buttons and set their isclicked property to false if necessary.
                    foreach (Button otherButton in btn)
                    {
                        ButtonCustom otherButtonCustom = otherButton.gameObject.GetComponent<ButtonCustom>();
                        if (otherButtonCustom != null && otherButtonCustom.isclicked)
                        {
                            otherButtonCustom.isclicked = false;

                            // Also reset the color of the other button.
                            ColorBlock otherButtonColors = otherButton.colors;
                            otherButtonColors.normalColor = Color.white;
                            otherButton.colors = otherButtonColors;
                        }
                    }

                    // Now it's safe to set the current button's isclicked property to true.
                    ButtonCustom buttonCustom = button.gameObject.GetComponent<ButtonCustom>();
                    if (buttonCustom != null)
                    {
                        buttonCustom.isclicked = true;
                        hasTriggeredNextQuestion = false;
                        // Change the color of the button to blue.
                        ColorBlock buttonColors = button.colors;
                        buttonColors.normalColor = Color.white;
                        button.colors = buttonColors;
                    }
                }
            }
            else if (btn.Count >= 1)
            {
                foreach (Button b in btn)
                {
                    if (b.gameObject.GetComponent<ButtonCustom>().isclicked == false)
                    {
                        ColorBlock colorBlock = b.colors;
                        colorBlock.normalColor = Color.white;
                        b.colors = colorBlock;
                    }
                }
            }

        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }
}