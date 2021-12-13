using UnityEngine;
using UnityEngine.Assertions;

public class HandAnimationController : MonoBehaviour
{
    [SerializeField] Hand hand;
    [SerializeField] string triggerAxisName = "RTrigger";
    [SerializeField] string gripAxisName = "RGrip";

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
    }

}
