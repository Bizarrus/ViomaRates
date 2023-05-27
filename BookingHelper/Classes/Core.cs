using BookingHelper.Models.JSON;
using BookingHelper.Networking;
using BookingHelper.UI;
using System;
using System.Collections.Generic;

namespace BookingHelper.Classes {
	public class Core : System.Windows.Application {
		private Feed feed;
		private Rates rates;
		private MainWindow window;

		public Core() {
			this.feed = new Feed();
			this.rates = new Rates();
			this.window = new MainWindow(this);

			// Connects to the Rooms-API
			this.feed.Connect((Action)(() => {
				window.Refresh(null);
				window.Dispatcher.Invoke(() => {
					window.Show();
				});
			}));
		}

		public void DisplayRates(RequestSheet sheet) {
			this.rates.Connect(sheet, (Action<IList<SheetEntry>>)((rates) => {
				// Showing UI, when ready
				window.Refresh(rates);
				window.Dispatcher.Invoke(() => {
					window.Show();
				});
			}));
		}

		public void Start() {
			this.Run();
		}

		public Feed GetFeed() {
			return this.feed;
		}
	}
}
