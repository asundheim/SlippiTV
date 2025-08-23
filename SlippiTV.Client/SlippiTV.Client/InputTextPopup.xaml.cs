using SlippiTV.Shared;

namespace SlippiTV.Client;

public partial class InputTextPopup
{
	public InputTextPopup(string placeholderText, string title)
	{
		InitializeComponent();

        this.PlaceholderText = placeholderText;
        this.Title = title;


        Loaded += InputTextPopup_Loaded;
	}

    private void InputTextPopup_Loaded(object? sender, EventArgs e)
    {
        this.BindingContext = this;
    }

    public string PlaceholderText { get; set; }
    public string Title { get; set; }
    public string ErrorText 
    { 
        get;
        set 
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(ErrorText));
            }
        }
    } = string.Empty;

    private const string _invalidConnectCodeErrorText = "Invalid connect code format";

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(string.Empty);
    }

    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        string rawCode = ConnectCodeEntry.Text;
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