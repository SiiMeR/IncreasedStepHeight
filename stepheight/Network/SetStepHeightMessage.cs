using ProtoBuf;

namespace stepheight.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class SetStepHeightMessage
{
    public float StepHeight { get; }


    private SetStepHeightMessage() { }

    public SetStepHeightMessage(float stepHeight)
    { StepHeight = stepHeight; }
}