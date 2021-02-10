using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using WalletWasabi.Blockchain.Keys;
using WalletWasabi.Fluent.ViewModels.AddWallet;
using WalletWasabi.Fluent.ViewModels.Login.PasswordFinder;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Fluent.ViewModels.Wallets;
using WalletWasabi.Userfacing;

namespace WalletWasabi.Fluent.ViewModels.Login
{
	[NavigationMetaData(Title = "Login")]
	public partial class LoginViewModel : RoutableViewModel
	{
		[AutoNotify] private string _password;
		[AutoNotify] private bool _isPasswordIncorrect;
		[AutoNotify] private bool _isPasswordNeeded;
		[AutoNotify] private string _walletName;

		public LoginViewModel(WalletViewModelBase walletViewModelBase)
		{
			WalletViewModelBase = walletViewModelBase;
			KeyManager = walletViewModelBase.Wallet.KeyManager;
			IsPasswordNeeded = !KeyManager.IsWatchOnly;
			_walletName = walletViewModelBase.WalletName;
			_password = "";
			var wallet = walletViewModelBase.Wallet;

			NextCommand = ReactiveCommand.CreateFromTask(async () =>
			{
				IsPasswordIncorrect = await Task.Run(async () =>
				{
					if (!IsPasswordNeeded)
					{
						return false;
					}

					if (PasswordHelper.TryPassword(KeyManager, Password, out var compatibilityPasswordUsed))
					{
						if (compatibilityPasswordUsed is { })
						{
							await ShowErrorAsync(PasswordHelper.CompatibilityPasswordWarnMessage, "Compatibility password was used");
						}

						return false;
					}

					return true;
				});

				if (!IsPasswordIncorrect)
				{

					var legalChecker = (await NavigationManager.MaterialiseViewModel(LegalDocumentsViewModel.MetaData) as LegalDocumentsViewModel)?.LegalChecker;

					if (legalChecker is { } lc && lc.TryGetNewLegalDocs(out _))
					{
						var legalDocs = new TermsAndConditionsViewModel(legalChecker, LoginWallet);
						Navigate(NavigationTarget.DialogScreen).To(legalDocs);
					}
					else
					{
						LoginWallet();
					}
				}
			});

			OkCommand = ReactiveCommand.Create(() =>
			{
				Password = "";
				IsPasswordIncorrect = false;
			});

			ForgotPasswordCommand = ReactiveCommand.Create(() =>
				Navigate(NavigationTarget.DialogScreen).To(new PasswordFinderIntroduceViewModel(wallet)));

			EnableAutoBusyOn(NextCommand);
		}

		private WalletViewModelBase WalletViewModelBase { get; }

		public ICommand OkCommand { get; }

		public ICommand ForgotPasswordCommand { get; }

		public KeyManager KeyManager { get; }

		private void LoginWallet()
		{
			WalletViewModelBase.Wallet.Login();
			WalletViewModelBase.RaisePropertyChanged(nameof(WalletViewModelBase.IsLoggedIn));
			Navigate().To(WalletViewModelBase, NavigationMode.Clear);
		}
	}
}
