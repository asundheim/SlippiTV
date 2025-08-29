namespace SlippiTV.Client.Views;

public partial class InfoPopup
{
	public InfoPopup(string infoText)
	{
		InitializeComponent();
        InfoText = infoText;
        BindingContext = this;
    }

	public string InfoText 
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