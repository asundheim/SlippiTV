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

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(string.Empty);
    }

    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(ConnectCodeEntry.Text);
    }
}