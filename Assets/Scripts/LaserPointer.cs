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
    public Button btn_NEXT;
    public Button btn_PLUS;
    public Button btn_MINUS;
    public Slider slider;
    public Controller controller;
    public TLXQuestionnaire tLX;
    private bool hasTriggeredNextQuestion = false;
    private bool nextButtonCooldown = false; // Flag to indicate if the "Next" button is on cooldown
    public float nextButtonCooldownDuration = 6.0f; // Duration of the cooldown period in seconds
    private float nextButtonCooldownTimer = 0f; // Timer to track the cooldown period

    private ColorBlock initialButtonColors; // Store the initial colors of the button
    private bool lastFramePrimary2DAxisClick = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        // Store the initial colors of the button
        initialButtonColors = btn_NEXT.colors;
    }

    void Update()
    {
        // Check if primary 2D axis has been clicked this frame
        bool thisFramePrimary2DAxisClick = controller.primary2DAxisClick;

        // Calculate flags for the start and end of a click
        bool clickStartedThisFrame = thisFramePrimary2DAxisClick && !lastFramePrimary2DAxisClick;
        bool clickEndedThisFrame = !thisFramePrimary2DAxisClick && lastFramePrimary2DAxisClick;

        // Check if the "Next" button is on cooldown
        if (nextButtonCooldown)
        {
            // Update the cooldown timer
            nextButtonCooldownTimer += Time.deltaTime;

            // Check if the cooldown period has ended
            if (nextButtonCooldownTimer >= nextButtonCooldownDuration)
            {
                // Reset the cooldown flag and timer
                nextButtonCooldown = false;
                nextButtonCooldownTimer = 0f;

                // Enable the "Next" button after the cooldown period
                btn_NEXT.interactable = true;
            }
            else
            {
                // Disable the "Next" button during the cooldown period
                btn_NEXT.interactable = false;
            }
        }

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
                if (button != null)
                {
                    ColorBlock colorBlock = button.colors;
                    colorBlock.normalColor = Color.green; // Change the color to green
                    button.colors = colorBlock;
                }

                if (clickStartedThisFrame)
                {
                    if (hit.collider.gameObject.name == "NEXT")
                    {
                        if (!hasTriggeredNextQuestion && !nextButtonCooldown)
                        {
                            tLX.NextQuestion();
                            hasTriggeredNextQuestion = true;
                            nextButtonCooldown = true;
                        }
                    }
                    else if (hit.collider.gameObject.name == "PLUS")
                    {
                        // Increase the slider's value when the "PLUS" button is pressed, but do not exceed the maximum value
                        slider.value = Mathf.Min(slider.value + 1f, slider.maxValue);
                    }
                    else if (hit.collider.gameObject.name == "MINUS")
                    {
                        // Decrease the slider's value when the "MINUS" button is pressed, but do not go below the minimum value
                        slider.value = Mathf.Max(slider.value - 1f, slider.minValue);
                    }

                    ButtonCustom buttonCustom = button.gameObject.GetComponent<ButtonCustom>();
                    if (buttonCustom != null)
                    {
                        buttonCustom.isclicked = true;
                        hasTriggeredNextQuestion = false;
                        ColorBlock buttonColors = button.colors;
                        buttonColors.normalColor = Color.white;
                        button.colors = buttonColors;
                    }
                }
            }
            else
            {
                ResetButtonColor(btn_NEXT);
                ResetButtonColor(btn_PLUS);
                ResetButtonColor(btn_MINUS);
            }
        }
        else
        {
            ResetButtonColor(btn_NEXT);
            ResetButtonColor(btn_PLUS);
            ResetButtonColor(btn_MINUS);

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }

        // Store the current primary 2D axis click state for the next frame
        lastFramePrimary2DAxisClick = thisFramePrimary2DAxisClick;
    }

    void ResetButtonColor(Button button)
    {
        if (button != null)
        {
            ColorBlock buttonColors = button.colors;
            buttonColors.normalColor = initialButtonColors.normalColor;
            button.colors = buttonColors;
        }
    }
}
