using UnityEngine;
using System.Collections;

public class ActionChild : ActionMaster
{
    public override void DoDuringStart()
    {
        base.DoDuringStart();
    }

    public override void DoDuringUpdate()
    {
        base.DoDuringUpdate();
    }

    public override void DoOnTrigger()
    {
        Debug.Log("Response from ActionChild");
    }
}
