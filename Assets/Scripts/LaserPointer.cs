using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VarjoExample;
using Telexistence;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Collections;

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
    public List<Button> shape = new List<Button>();
    public List<Button> color = new List<Button>();

    // Define the NEXT_TASK button
    public Button btn_NEXT_TASK;

    // Task-related variables
    private int currentTask = 0;
    public string shapeSelected;
    public string colorSelected;

    private Button lastButtonHit;

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
    private bool hasExecuted = false;

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

        foreach (var b in shape)
        {
            if (shapeSelected != null && shapeSelected == b.gameObject.name)
            {
                ColorBlock colorBlock = b.colors;
                colorBlock.normalColor = Color.green;
                b.colors = colorBlock;
            }
            else
            {
                ColorBlock colorBlock = b.colors;
                colorBlock.normalColor = Color.white;
                b.colors = colorBlock;
            }
        }

        foreach (var b in color)
        {
            if (colorSelected != null && colorSelected == b.gameObject.name)
            {
                ColorBlock colorBlock = b.colors;
                colorBlock.normalColor = Color.green; // Change the color to green
                b.colors = colorBlock;
            }
            else
            {
                ColorBlock colorBlock = b.colors;
                colorBlock.normalColor = Color.white;
                b.colors = colorBlock;
            }
        }

        if (mm.CurrentTask == modalities.TaskType.start)
        {
            // All tasks are completed, so change the button label to "SAVE"
            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "TASK 1";
        }
        else if (mm.CurrentTask == modalities.TaskType.t1)
        {
            // All tasks are completed, so change the button label to "SAVE"
            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "TASK 2";
        }
        else if (mm.CurrentTask == modalities.TaskType.t2)
        {
            // All tasks are completed, so change the button label to "SAVE"
            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "TASK 3";
        }
        else if (mm.CurrentTask == modalities.TaskType.t3)
        {
            // There are still tasks remaining, so keep the button label as "NEXT TASK"
            btn_NEXT_TASK.GetComponentInChildren<TextMeshProUGUI>().text = "SAVE";
        }

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
                    if (!shape.Contains(button) && !color.Contains(button))
                    {
                        ColorBlock colorBlock = button.colors;
                        colorBlock.normalColor = Color.green; // Change the color to green
                        button.colors = colorBlock;
                    }
                }

                if (controller.primary2DAxisClick && !hasExecuted)
                {
                    if (shape.Contains(button))
                    {
                        shapeSelected = button.gameObject.name;
                    }

                    if (color.Contains(button))
                    {
                        colorSelected = button.gameObject.name;
                    }

                    CheckAndExecuteButtonFunction("NEXT_TASK", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("NEXT", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("PLUS", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("MINUS", hit, clickStartedThisFrame);
                    CheckAndExecuteButtonFunction("PHOTO", hit, clickStartedThisFrame);

                    hasExecuted = true;
                }

                lastButtonHit = button;
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
                    if (mm.CurrentTask == modalities.TaskType.start)
                    {
                        mm.CurrentTask = modalities.TaskType.t1;
                        hasExecuted = false;
                    }
                    else if (mm.CurrentTask == modalities.TaskType.t1)
                    {
                        mm.CurrentTask = modalities.TaskType.t2;
                        hasExecuted = false;
                    }
                    else if (mm.CurrentTask == modalities.TaskType.t2)
                    {
                        mm.CurrentTask = modalities.TaskType.t3;
                        hasExecuted = false;
                    }
                    else if (mm.CurrentTask == modalities.TaskType.t3)
                    {
                        mm.CurrentTask = modalities.TaskType.start;
                        hasExecuted = false;
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
                    // Get the texture from the source image
                    Texture2D sourceTexture = sourceImage.texture as Texture2D;

                    // Calculate the middle point of the source image
                    int middleX = sourceTexture.width / 2;
                    int middleY = sourceTexture.height / 2;

                    // Define the size of the area to copy
                    int size = 200;

                    // Calculate the starting point to copy from
                    int startX = middleX - size / 2;
                    int startY = middleY - size / 2;

                    // Get the pixels from the source image
                    Color[] pixels = sourceTexture.GetPixels(startX, startY, size, size);

                    // Create a new texture and set the pixels
                    Texture2D targetTexture = new Texture2D(size, size);
                    targetTexture.SetPixels(pixels);
                    targetTexture.Apply();

                    // Set the texture to the target image
                    targetImage.texture = targetTexture;

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