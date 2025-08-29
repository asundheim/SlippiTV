namespace SlippiTV.Client.Views;

public partial class SelectableListViewPopup
{
	public SelectableListViewPopup(List<string> selectionOptions)
	{
		InitializeComponent();

		// a bit lazy but it works fine
		this.SelectionCollectionView.ItemsSource = selectionOptions;
	}

    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(string.Join(";", this.SelectionCollectionView.SelectedItems.Cast<string>()));
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await CloseAsync(string.Empty);
    }
}