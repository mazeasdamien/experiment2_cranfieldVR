using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VarjoExample;
using Telexistence;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class LaserPointer : MonoBehaviour
{
    public float maxDistance = 5f; // Maximum distance of the laser pointer
    private LineRenderer lineRenderer;

    public Material originalMaterial;
    public Button btn_NEXT;
    public Button btn_PLUS;
    public Button btn_MINUS;
    public Slider slider;

    // Define buttons for the shapes and colors
    public Button btn_CIRCLE;
    public Button btn_SQUARE;
    public Button btn_TRIANGLE;
    public Button btn_STAR;
    public Button btn_BLUE;
    public Button btn_GREEN;
    public Button btn_YELLOW;
    public Button btn_RED;

    // Define the NEXT_TASK button
    public Button btn_NEXT_TASK;

    // Task-related variables
    private int currentTask = 0;
    private bool shapeSelected = false;
    private bool colorSelected = false;


    public Controller controller;
    public TLXQuestionnaire tLX;
    public modalities mm;
    private bool hasTriggeredNextQuestion = false;
    private bool nextButtonCooldown = false; // Flag to indicate if the "Next" button is on cooldown
    public float nextButtonCooldownDuration = 6.0f; // Duration of the cooldown period in seconds
    private float nextButtonCooldownTimer = 0f; // Timer to track the cooldown period

    public Button btn_PHOTO; // Add a new button
    public RawImage sourceImage; // Source image
    public RawImage targetImage; // Target image

    private ColorBlock initialButtonColors; // Store the initial colors of the button
    private ColorBlock initialNextTaskButtonColors; // Store the initial colors of the next task button
    private bool lastFramePrimary2DAxisClick = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        // Store the initial colors of the button
        initialButtonColors = btn_NEXT.colors;
        initialNextTaskButtonColors = btn_NEXT_TASK.colors;
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
                    if (hit.collider.gameObject.name.Contains("Shape"))
                    {
                        // A shape button has been clicked, so record that a shape has been selected for the current task
                        shapeSelected = true;
                    }
                    else if (hit.collider.gameObject.name.Contains("Color"))
                    {
                        // A color button has been clicked, so record that a color has been selected for the current task
                        colorSelected = true;
                    }

                    // Check and execute the functions of each button based on the GameObject's name
                    CheckAndExecuteButtonFunction("NEXT_TASK", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("NEXT", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("PLUS", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("MINUS", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("PHOTO", hit, clickStartedThisFrame);
                }
            }
            else
            {
                ResetButtonColor(btn_NEXT);
                ResetButtonColor(btn_PLUS);
                ResetButtonColor(btn_MINUS);
                ResetButtonColor(btn_PHOTO);
            }
        }
        else
        {
            ResetButtonColor(btn_NEXT);
            ResetButtonColor(btn_PLUS);
            ResetButtonColor(btn_MINUS);
            ResetButtonColor(btn_PHOTO);

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }

        // Store the current primary 2D axis click state as the last frame's state for the next frame
        lastFramePrimary2DAxisClick = thisFramePrimary2DAxisClick;
    }



    private void CheckAndExecuteButtonFunction(string buttonName, RaycastHit hit, bool clickStartedThisFrame)
    {
        if (hit.collider.gameObject.name == buttonName)
        {
            switch (buttonName)
            {
                case "NEXT_TASK":
                    if (currentTask < 3 && shapeSelected && colorSelected)
                    {
                        // Both shape and color have been selected for the current task, so move to the next task
                        currentTask++;

                        // Reset shape and color selection for the next task
                        shapeSelected = false;
                        colorSelected = false;

                        if (currentTask == 1)
                        {
                            // All tasks are completed, so change the button label to "SAVE"
                            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "START";
                        }
                        else if (currentTask == 2)
                        {
                            // All tasks are completed, so change the button label to "SAVE"
                            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "NEXT TASK";
                        }
                        else
                        {
                            // There are still tasks remaining, so keep the button label as "NEXT TASK"
                            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "SAVE";
                        }
                    }
                    break;
                case "NEXT":
                    if (!hasTriggeredNextQuestion && !nextButtonCooldown)
                    {
                        tLX.NextQuestion();
                        hasTriggeredNextQuestion = true;
                        nextButtonCooldown = true;
                    }
                    break;
                case "PLUS":
                    // Increase the slider's value when the "PLUS" button is pressed, but do not exceed the maximum value
                    slider.value = Mathf.Min(slider.value + 1f, slider.maxValue);
                    break;
                case "MINUS":
                    // Decrease the slider's value when the "MINUS" button is pressed, but do not go below the minimum value
                    slider.value = Mathf.Max(slider.value - 1f, slider.minValue);
                    break;
                case "PHOTO":
                    // Perform the photo capturing and saving process
                    break;
            }

            if (clickStartedThisFrame)
            {
                Button button = hit.collider.gameObject.GetComponent<Button>();
                if (button != null)
                {
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
        }
    }

    private void ResetButtonColor(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = Color.white;
        button.colors = colorBlock;
    }
}