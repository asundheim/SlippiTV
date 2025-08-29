namespace SlippiTV.Client.Views;

public partial class ErrorPopup
{
	public ErrorPopup(string errorText)
	{
		InitializeComponent();
        ErrorText = errorText;
        BindingContext = this;
    }

	public string ErrorText 
	{ 
		get; 
		set 
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
		await this.CloseAsync(string.Empty);
    }
}