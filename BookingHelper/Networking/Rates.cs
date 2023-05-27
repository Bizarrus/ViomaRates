using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using BookingHelper.Classes;
using BookingHelper.Models.JSON;
using BookingHelper.UI;
using Newtonsoft.Json;

namespace BookingHelper.Networking {
	public class Rates {
		private async void Login(String username, String password, Action<String, User> success, Action<String> error) {
			HttpClientHandler handler   = new HttpClientHandler();
			HttpClient client           = new HttpClient(handler);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response = await client.PostAsync("https://cst-pro.viomassl.com/api/v6/user/login", new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			}));

			HttpContent content = response.Content;

			if((int)response.StatusCode == 200) {
				string result       = await content.ReadAsStringAsync();
				LoginResult data    = JsonConvert.DeserializeObject<LoginResult>(result);

				if(data.User.Inactive) {
					error.Invoke("Vioma Account error: Account inactive!");
				} else if(data.User.PassForceChange) {
					error.Invoke("Vioma Account error: Password must be changed!");
				} else if(data.User.Deleted) {
					error.Invoke("Vioma Account error: Account marked as deleted");
				} else if(data.User.Disabled) {
					error.Invoke("Vioma Account error: Account is disabled!");
				} else if(data.Error != "") {
					error.Invoke($"Vioma Login error: ({data.Error})");
				} else {
					success.Invoke(data.Token, data.User);
				}
			} else {
				error.Invoke($"Vioma Connection error: Status Code ({response.StatusCode})");
			}
		}
		public async void GetRates(string token, RequestSheet sheet, Action<IList<SheetEntry>> action) {
			if(!Config.Exists("Hotel", "ID")) {
				MessageBox.Show($"Settings.ini: The Value of <ID> on <Hotel> section is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if(!Config.Exists("Endpoint", "ClientID")) {
				MessageBox.Show($"Settings.ini: The Value of <ClientID> on <Endpoint> section is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			HttpClientHandler handler = new HttpClientHandler();
			HttpClient client = new HttpClient(handler);
			client.DefaultRequestHeaders.Add("authorization", "Bearer " + token);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

			HttpResponseMessage response = await client.PostAsync("https://cst-pro.viomassl.com/api/v6/hotel/" + Config.Get("Hotel", "ID")  + "/data/sheet", new StringContent(JsonConvert.SerializeObject(sheet, Formatting.None).ToString()));
			HttpContent content = response.Content;

			if((int)response.StatusCode == 200) {
				string result            = await content.ReadAsStringAsync();

				try {
					IList<SheetEntry> data = JsonConvert.DeserializeObject<IList<SheetEntry>>(result);
					action.Invoke(data);
				} catch(Exception e) {
					var dialogType  = typeof(Form).Assembly.GetType("System.Windows.Forms.PropertyGridInternal.GridErrorDlg");
					var dialog      = (Form)Activator.CreateInstance(dialogType, new PropertyGrid());
					dialog.Text		= "Vioma Error (Rates)";
					dialog.Controls.Find("okBtn", true)[0].Visible = false;
					dialogType.GetProperty("Details").SetValue(dialog, e.StackTrace, null);
					dialogType.GetProperty("Message").SetValue(dialog, "Unhandled Error:\r\n" + e.Message, null);
					dialog.ShowDialog();
				}
			} else {
				var dialogType		= typeof(Form).Assembly.GetType("System.Windows.Forms.PropertyGridInternal.GridErrorDlg");
				var dialog			= (Form)Activator.CreateInstance(dialogType, new PropertyGrid());
				dialog.Text			= "Vioma Error (Rates)";
				dialog.Controls.Find("okBtn", true)[0].Visible = false;
				dialogType.GetProperty("Details").SetValue(dialog, content, null);
				dialogType.GetProperty("Message").SetValue(dialog, $"Vioma Connection error: Status Code({response.StatusCode})", null);
				dialog.ShowDialog();
			}
		}

		public void Connect(RequestSheet sheet, Action<IList<SheetEntry>> action) {
			if(!Config.Exists("User", "Password")) {
				Login window = new Login();
				
				if(Config.Exists("User", "Username")) {
					window.Username.Text = Config.Get("User", "Username");
				}
				
				window.Open(() => {
					this.Connect(sheet, action);
				});
				return;
			}

			Login(Config.Get("User", "Username"), Config.Get("User", "Password"), (token, user) => {
				GetRates(token, sheet, (rates) => {
					action.Invoke(rates);
				});
			}, (error) => {
				MessageBox.Show($"ERROR at Auth: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			});
		}
	}
}
