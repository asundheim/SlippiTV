using SlippiTV.Shared;

namespace SlippiTV.Client.Views;

public partial class InputConnectCodePopup : InputTextPopup
{
    public InputConnectCodePopup(string title) : base("Enter Connect Code (ABC#123)", title)
    {
    }

    private const string _invalidConnectCodeErrorText = "Invalid connect code format";
    protected override async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        string rawCode = InputText;
        if (ConnectCodeUtils.IsValidConnectCode(rawCode))
        {
            await CloseAsync(ConnectCodeUtils.NormalizeConnectCode(rawCode));
        }
        else
        {
            ErrorText = _invalidConnectCodeErrorText;
        }
    }
}
