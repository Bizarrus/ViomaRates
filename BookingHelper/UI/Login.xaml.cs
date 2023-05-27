using BookingHelper.Classes;
using System;
using System.Windows;

namespace BookingHelper.UI {
	public partial class Login : Window {
		private Action action = null;

		public Login() {
			InitializeComponent();
		}

		public void Open(Action action) {
			this.action = action;
			this.Show();
		}

		void OnClick(object sender, RoutedEventArgs e) {
			string username = this.Username.Text;
			string password = this.Password.Password;

			if(this.Save.IsChecked == true) {
				Config.Set("User", "Username", username);
				Config.Set("User", "Password", password);
			}

			Config.SetTemporary("User", "Username", username);
			Config.SetTemporary("User", "Password", password);

			this.Close();
			this.action.Invoke();
		}
	}
}
