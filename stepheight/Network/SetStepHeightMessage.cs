using ProtoBuf;

namespace stepheight.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class SetStepHeightMessage
{
    public float StepHeight { get; }
    public bool IsInitialSync { get; }


    private SetStepHeightMessage() { }

    public SetStepHeightMessage(float stepHeight, bool isInitialSync = false)
    { StepHeight = stepHeight;
        IsInitialSync = isInitialSync;
    }
}