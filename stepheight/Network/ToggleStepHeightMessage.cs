using ProtoBuf;

namespace stepheight.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class ToggleStepHeightMessage
{
    public bool IsEnabled { get; }
    public bool IsInitialSync { get; }
        
    private ToggleStepHeightMessage() { }

    public ToggleStepHeightMessage(bool isEnabled, bool isInitialSync = false)
    { IsEnabled = isEnabled;
        IsInitialSync = isInitialSync;
    }
}