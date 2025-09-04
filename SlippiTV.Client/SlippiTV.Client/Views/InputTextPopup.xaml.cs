using CommunityToolkit.Maui.Views;

namespace SlippiTV.Client.Views;

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

    protected string InputText => InputTextEntry.Text;

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(string.Empty);
    }

    protected virtual async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        await this.CloseAsync(InputText);
    }
}