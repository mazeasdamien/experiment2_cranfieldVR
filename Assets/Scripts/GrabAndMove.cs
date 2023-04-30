using UnityEngine;
using Leap;
using Leap.Unity;

public class GrabAndMove : MonoBehaviour
{
    [SerializeField]
    private LeapServiceProvider leapServiceProvider;
    [SerializeField]
    private float grabDistance = 0.1f;

    private Controller leapController;
    private Hand leftHand;
    private Hand rightHand;
    private bool isGrabbed;

    [SerializeField]
    private Color hoverColor = Color.yellow;
    [SerializeField]
    private Color grabColor = Color.green;
    private Color initialColor;

    private void Start()
    {
        leapController = leapServiceProvider.GetLeapController();
        initialColor = GetComponent<Renderer>().material.color;
    }

    private void Update()
    {
        Frame frame = leapController.Frame();
        leftHand = frame.Hands.Find(hand => hand.IsLeft);
        rightHand = frame.Hands.Find(hand => hand.IsRight);

        if (IsHandGrabbing(leftHand) || IsHandGrabbing(rightHand))
        {
            if (!isGrabbed)
            {
                Vector3 closestHandPosition = GetClosestHandPosition();
                float distance = Vector3.Distance(transform.position, closestHandPosition);

                if (distance < grabDistance)
                {
                    isGrabbed = true;
                }
            }
        }
        else
        {
            isGrabbed = false;
        }

        if (isGrabbed)
        {
            Vector3 newPosition = GetClosestHandPosition();
            transform.position = newPosition;
            GetComponent<Renderer>().material.color = grabColor;
        }
        else if (IsWithinGrabRange())
        {
            GetComponent<Renderer>().material.color = hoverColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = initialColor;
        }
    }

    private bool IsWithinGrabRange()
    {
        Vector3 closestHandPosition = GetClosestHandPosition();
        float distance = Vector3.Distance(transform.position, closestHandPosition);
        return distance < grabDistance;
    }

    private bool IsHandGrabbing(Hand hand)
    {
        if (hand == null) return false;
        return hand.GrabStrength > 0.5f;
    }

    private Vector3 GetClosestHandPosition()
    {
        if (leftHand == null) return LeapToUnityVector3(rightHand.PalmPosition);
        if (rightHand == null) return LeapToUnityVector3(leftHand.PalmPosition);

        Vector3 leftHandUnityPosition = LeapToUnityVector3(leftHand.PalmPosition);
        Vector3 rightHandUnityPosition = LeapToUnityVector3(rightHand.PalmPosition);
        Vector3 objectPosition = transform.position;

        float leftHandDistance = Vector3.Distance(objectPosition, leftHandUnityPosition);
        float rightHandDistance = Vector3.Distance(objectPosition, rightHandUnityPosition);

        return leftHandDistance < rightHandDistance ? leftHandUnityPosition : rightHandUnityPosition;
    }

    private Vector3 LeapToUnityVector3(Vector3 leapVector)
    {
        return leapServiceProvider.transform.TransformPoint(new Vector3(leapVector.x, leapVector.y, leapVector.z));
    }
}