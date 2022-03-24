using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace WalletWasabi.Fluent.Views.SearchBar;

public class SearchBarTextPart : UserControl
{
	public SearchBarTextPart()
	{
		InitializeComponent();
	}

	private void RootPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		e.Handled = false;
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}