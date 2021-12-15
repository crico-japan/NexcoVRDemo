using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class HandAnimationController : MonoBehaviour
{
    [SerializeField] Hand hand;
    [SerializeField] string triggerAxisName = "RTrigger";
    [SerializeField] string gripAxisName = "RGrip";
    [SerializeField] string cancelButtonName = "Cancel";
    [SerializeField] string submitButtonName = "Submit";

    [SerializeField] UnityEvent onCancelButtonDown = new UnityEvent();
    [SerializeField] UnityEvent onSubmitButtonDown = new UnityEvent();

    private void Awake()
    {
        Assert.IsNotNull(hand);
    }

    void Update()
    {
        float trigger = Input.GetAxis(triggerAxisName);
        float grip = Input.GetAxis(gripAxisName);

        hand.SetTrigger(trigger);
        hand.SetGrip(grip);

        if (Input.GetButtonDown(cancelButtonName))
            onCancelButtonDown.Invoke();

        if (Input.GetButtonDown(submitButtonName))
            onSubmitButtonDown.Invoke();
    }

}
