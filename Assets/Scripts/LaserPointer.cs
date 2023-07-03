using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VarjoExample;
using Telexistence;
using System.Collections.Generic;
using System.IO;

public class LaserPointer : MonoBehaviour
{
    public float maxDistance = 5f;
    private LineRenderer lineRenderer;
    public Material originalMaterial;
    public Button btn_NEXT;
    public Button btn_PLUS;
    public Button btn_MINUS;
    public Slider slider;
    public List<Button> shape = new List<Button>();
    public List<Button> color = new List<Button>();
    public Button btn_NEXT_TASK;
    public string shapeSelected;
    public string colorSelected;
    public Controller controller;
    public TLXQuestionnaire tLX;
    public modalities mm;
    private bool nextButtonCooldown = false;
    public float nextButtonCooldownDuration = 0.5f;
    private float nextButtonCooldownTimer = 0f;
    public Button btn_PHOTO;
    public RawImage sourceImage;
    public RawImage targetImage;
    private bool buttonClicked = false;
    private bool buttonClickedQuestionnaire = false;
    private bool photoTaken = false;
    public meshKinect mk;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {

        if (mm.CurrentTask != modalities.TaskType.start)
        {
            {
                if (shapeSelected != null && colorSelected != null && photoTaken == true)
                    btn_NEXT_TASK.gameObject.SetActive(true);
                else
                    btn_NEXT_TASK.gameObject.SetActive(false);
            }
        }
        else
        {
            btn_NEXT_TASK.gameObject.SetActive(true);
        }

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

        if (shapeSelected != null && colorSelected != null)
        {
            buttonClicked = false;
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

                if (controller.primary2DAxisClick)
                {

                    if (shape.Contains(button))
                    {
                        shapeSelected = button.gameObject.name;
                    }

                    if (color.Contains(button))
                    {
                        colorSelected = button.gameObject.name;
                    }

                    switch (button.name)
                    {
                        case "NEXT_TASK":
                            if (mm.CurrentTask != modalities.TaskType.start)
                            {
                                if (shapeSelected != null && colorSelected != null)
                                {
                                    if (!buttonClicked)
                                    { 
                                        if (mm.CurrentModality != modalities.ModalityType.TRIAL)
                                        {
                                            if (mm.CurrentTask == modalities.TaskType.t1 || mm.CurrentTask == modalities.TaskType.t2 || mm.CurrentTask == modalities.TaskType.t3)
                                            {
                                                Texture2D targetTexture1 = targetImage.texture as Texture2D;
                                                if (targetTexture1 != null)
                                                {
                                                    byte[] bytes = targetTexture1.EncodeToJPG();
                                                    string filename = mm.par_ID + "_" + mm.CurrentModality + "_" + mm.CurrentTask + "_" + mk.midDepthInCm.ToString() + "cm" + ".jpg";
                                                    string folderPath = Path.Combine(Application.dataPath, "Participants_data", $"participant_{mm.par_ID}");
                                                    Directory.CreateDirectory(folderPath);

                                                    string filePath = Path.Combine(folderPath, filename);
                                                    File.WriteAllBytes(filePath, bytes);
                                                }
                                            }
                                        }

                                        mm.NextTask();
                                        shapeSelected = null;
                                        colorSelected = null;
                                        ResetTargetTexture();
                                        photoTaken = false;
                                        buttonClicked = true;
                                    }
                                }
                            }
                            else
                            {
                                mm.NextTask();
                                buttonClicked = true;
                            }
                            break;
                        case "NEXT":
                            if (!nextButtonCooldown)
                            {
                                tLX.NextQuestion();
                                nextButtonCooldown = true;
                            }
                            break;
                        case "PLUS":
                            if (!buttonClickedQuestionnaire)
                            {
                                slider.value = Mathf.Min(slider.value + 1f, slider.maxValue);
                                buttonClickedQuestionnaire=true;
                            }
                            break;
                        case "MINUS":
                            if (!buttonClickedQuestionnaire)
                            {
                                slider.value = Mathf.Max(slider.value - 1f, slider.minValue);
                                buttonClickedQuestionnaire=true;
                            }
                            break;
                        case "PHOTO":
                            photoTaken = true;
                            Texture2D sourceTexture = sourceImage.texture as Texture2D;
                            int middleX = sourceTexture.width / 2;
                            int middleY = sourceTexture.height / 2;
                            int size = 200;
                            int startX = middleX - size / 2;
                            int startY = middleY - size / 2;
                            Color[] pixels = sourceTexture.GetPixels(startX, startY, size, size);
                            Texture2D targetTexture = new Texture2D(size, size);
                            targetTexture.SetPixels(pixels);

                            // Draw a red cross in the center of the target texture
                            int crossSize = 15; // Change this to adjust the size of the cross
                            int crossThickness = 2; // Change this to adjust the thickness of the cross
                            Color crossColor = Color.red; // Change this to adjust the color of the cross
                            for (int i = -crossSize; i <= crossSize; i++)
                            {
                                for (int j = -crossThickness; j <= crossThickness; j++)
                                {
                                    // Vertical line
                                    targetTexture.SetPixel(size / 2 + j, size / 2 + i, crossColor);
                                    // Horizontal line
                                    targetTexture.SetPixel(size / 2 + i, size / 2 + j, crossColor);
                                }
                            }

                            targetTexture.Apply();
                            targetImage.texture = targetTexture;
                            break;
                    }
                }
                else
                { buttonClickedQuestionnaire = false; }
            }
        }
        else
        {
            buttonClicked = false;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    public string GetShapeColorPair()
    {
        return $"{shapeSelected},{colorSelected}";
    }

    public void ResetTargetTexture()
    {
        Texture2D targetTexture = targetImage.texture as Texture2D;
        if (targetTexture == null) return;

        Color[] whitePixels = new Color[targetTexture.width * targetTexture.height];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }

        targetTexture.SetPixels(whitePixels);
        targetTexture.Apply();
        targetImage.texture = targetTexture;
    }
}