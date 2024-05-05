using ProtoBuf;

namespace stepheight.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class ToggleStepHeightMessage
{
    public bool IsInitialSync { get; }
        
    private ToggleStepHeightMessage() { }

    public ToggleStepHeightMessage(bool isInitialSync = false)
    { 
        IsInitialSync = isInitialSync;
    }
}