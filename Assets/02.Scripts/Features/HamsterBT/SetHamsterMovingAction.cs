// 햄스터의 이동을 켜고 끄는 Behavior Graph 커스텀 액션
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Hamster Moving", story: "[Self] sets moving [IsMovingEnabled]", category: "Action", id: "b7d2e94af3164c05a81e6c2d9f403b51")]
public partial class SetHamsterMovingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<HamsterMoving> HamsterMoving;
    [SerializeReference] public BlackboardVariable<bool> IsMovingEnabled;

    protected override Status OnStart()
    {
        if (IsMovingEnabled.Value)
        {
            HamsterMoving.Value.SetMovingEnabled(true);
            return Status.Success;
        }

        HamsterMoving.Value.SetMovingEnabled(false);
        return WaitForStop();
    }

    protected override Status OnUpdate()
    {
        return WaitForStop();
    }

    private Status WaitForStop()
    {
        if (HamsterMoving.Value.IsStopPending)
        {
            return Status.Running;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}