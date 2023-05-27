using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BookingHelper.Classes;
using BookingHelper.Models.JSON;

namespace BookingHelper.UI {
	public partial class MainWindow : Window {
		private Core core;
		private bool updated = false;
		private ObservableCollection<ComboboxItem> roomTypes = new ObservableCollection<ComboboxItem>();
		private ObservableCollection<ComboboxItem> pensionTypes = new ObservableCollection<ComboboxItem>();
		private ObservableCollection<PriceEntry> priceList = new ObservableCollection<PriceEntry>();
		public MainWindow(Core core) {
			this.core = core;
			InitializeComponent();

			this.InputRooms.ItemsSource = this.roomTypes;
			this.InputPension.ItemsSource = this.pensionTypes;
		}

		public void Refresh(IList<SheetEntry> rates) {
			if(!this.updated) {
				// Set the current day
				this.InputArrival.Dispatcher.Invoke(() => {
					this.InputArrival.SelectedDate = DateTime.Now;
				});

				// Set by default the next day
				this.InputDeparture.Dispatcher.Invoke(() => {
					this.InputDeparture.SelectedDate = DateTime.Now.AddDays(1);
				});

				// Fill out room categories
				foreach(var room in this.core.GetFeed().GetRooms()) {
					this.InputRooms.Dispatcher.Invoke(() => {
						this.roomTypes.Add(new ComboboxItem {
							Text = room.GetText(),
							Name = room.GetName(),
							ID = room.GetID()
						});
					});
				}

				foreach(var type in this.core.GetFeed().GetCaterings()) {
					this.InputPension.Dispatcher.Invoke(() => {
						this.pensionTypes.Add(new ComboboxItem {
							Text = type.GetText(),
							Name = type.GetName(),
							ID = type.GetID()
						});
					});
				}

				// Select the first rows
				this.InputRooms.Dispatcher.Invoke(() => {
					this.InputRooms.SelectedIndex = 0;
				});

				this.InputPension.Dispatcher.Invoke(() => {
					this.InputPension.SelectedIndex = 0;
				});

				this.updated = true;
			}

			if(rates != null) {
				DateTime date;
				float price = 0;

				// Update Rate table
				this.PriceOverview.Dispatcher.Invoke(() => {
					this.priceList.Clear();
				});

				int persons = this.InputPersons.Value;

				foreach(SheetEntry entry in rates) {
					this.PriceOverview.Dispatcher.Invoke(() => {
						// Check if the selected room type is the same
						if(entry.RoomType == (int)((ComboboxItem)this.InputRooms.SelectedItem).ID) {

							// Check if price is not empty
							if(entry.RoomPrices.Count > 0 && entry.RoomPrices.ContainsKey("" + persons)) {

								// Check if persons section is not empty
								if(entry.RoomPrices["" + persons].ContainsKey("Price")) {
									DateTime.TryParseExact(entry.Date + "", "yyyyMMdd", null, DateTimeStyles.None, out date);
									bool available = false;

									/*if (entry.RestrictionClosed != null && entry.RestrictionClosed.ContainsKey("Value")) {
                                        available = !Boolean.Parse(entry.RestrictionClosed["Value"].ToString());
                                    } else*/
									if(entry.UnitsAvail.Count > 0 && entry.UnitsAvail.ContainsKey("Value")) {
										if(Int32.Parse(entry.UnitsAvail["Value"].ToString()) >= 1) {
											available = true;
										}
									}

									// Check if the selected catering type is the same
									if(entry.Rate == (int)((ComboboxItem)this.InputPension.SelectedItem).ID) {
										float dayPrice  = float.Parse(entry.RoomPrices["" + persons]["Price"].ToString());
										dayPrice += this.CalculateChildrenPrice(entry, this.InputChildrens.Text);
										price += dayPrice;

										this.priceList.Add(new PriceEntry {
											Available = "../Assets/" + (available ? "Available.png" : "Unavailable.png"),
											Date = date.ToString("dd.MM.yyyy"),
											Type = this.GetRoomType(entry.RoomType),
											Catering = this.GetCateringType(entry.Rate),
											Persons = this.ReturnPersons(persons, this.InputChildrens.Text),
											Price = String.Format("{0:N2} €", dayPrice)
										});
									}
								}
							}
						}

						// Update total price
						this.TotalPrice.Content = String.Format("{0:N2} €", price);

						NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
						queryString.Add("calendar_date_from", this.InputArrival.SelectedDate.Value.ToString("dd.MM.yyyy"));
						queryString.Add("calendar_date_to", this.InputDeparture.SelectedDate.Value.ToString("dd.MM.yyyy"));
						queryString.Add("persons_adults", persons.ToString());

						if(!string.IsNullOrEmpty(this.InputChildrens.Text)) {
							queryString.Add("persons_children_ages", this.InputChildrens.Text);
						}

						queryString.Add("submitbook", "Suchen & buchen");
						queryString.Add("step", "roomtypes");
						queryString.Add("c[id_hotel]", Config.Get("Hotel", "ID"));
						queryString.Add("items[]", "hrt:" + (int)((ComboboxItem)this.InputRooms.SelectedItem).ID);
						//queryString.Add("vri_id", "12896");

						// Update booking link
						this.BookingLink.Text = Config.Get("Endpoint", "BookingPage") + "?" + queryString.ToString();
					});
				}

				this.PriceOverview.ItemsSource = this.priceList.OrderBy(a => a.Date);
				this.Loading.Visibility = Visibility.Collapsed;

				if(this.PriceOverview.Items.Count == 0) {
					this.NoResults.Visibility = Visibility.Visible;
				}
			}
		}

		private float CalculateChildrenPrice(SheetEntry entry, string childrens) {
			string[] ages;
			float price = 0;

			if(childrens.Contains(',')) {
				ages = childrens.Split(',');
			} else {
				ages = new string[] { childrens };
			}

			if(ages.Length > 0) {
				foreach(string word in ages) {
					if(String.IsNullOrEmpty(word)) {
						continue;
					}

					int age = Int32.Parse(word);
					int id  = GetChildrenIDByAge(age);

					if(id == -1) {
						// Children not found, too old?
					} else if(entry.ChildrenPrices.ContainsKey(id + "/0") && entry.ChildrenPrices[id + "/0"].ContainsKey("Price")) {
						price += float.Parse(entry.ChildrenPrices["" + id + "/0"]["Price"].ToString());
					}
				}
			}

			return price;
		}
		private String ReturnPersons(int persons, string childrens) {
			string[] ages;
			Dictionary<int, int> categories = new Dictionary<int, int>();

			if(childrens.Contains(',')) {
				ages = childrens.Split(',');
			} else {
				ages = new string[] { childrens };
			}

			foreach(var child in this.core.GetFeed().GetChildrens()) {
				categories.Add(child.id, 0);
			}

			if(ages.Length > 0) {
				foreach(string word in ages) {
					if(String.IsNullOrEmpty(word)) {
						continue;
					}

					int age     = Int32.Parse(word);
					int id      = GetChildrenIDByAge(age);
					int value   = 0;

					if(id != -1) {
						if(categories.ContainsKey(id)) {
							value = categories[id];
							categories.Remove(id);
						}

						value += 1;
						categories.Add(id, value);
					}
				}
			}

			string result = persons.ToString();

			foreach(var entry in categories) {
				result += "/";
				result += entry.Value;
			}

			return result;
		}

		private int GetChildrenIDByAge(int age) {
			int id = -1;

			foreach(var child in this.core.GetFeed().GetChildrens()) {
				if(age >= child.from && age <= child.to) {
					id = child.id;
					break;
				}
			}

			return id;
		}

		private string GetRoomType(int id) {
			string name = "";

			foreach(var room in this.core.GetFeed().GetRooms()) {
				if(id == room.GetID()) {
					name = room.GetText();
					break;
				}
			}

			if(String.IsNullOrEmpty(name)) {
				return "#" + id;
			}

			return name;
		}

		private string GetCateringType(int id) {
			string name = "";

			foreach(var catering in this.core.GetFeed().GetCaterings()) {
				if(id == catering.GetID()) {
					name = catering.GetName();
					break;
				}
			}

			if(String.IsNullOrEmpty(name)) {
				return "#" + id;
			}

			return name;
		}

		void OnClick(object sender, RoutedEventArgs e) {
			this.Loading.Visibility = Visibility.Visible;
			this.NoResults.Visibility = Visibility.Collapsed;
			List<int> stays             = new List<int>();
			DateTime day                = this.InputArrival.SelectedDate.Value;

			do {
				stays.Add(Int32.Parse(day.ToString("yyyyMMdd")));
				day = day.AddDays(1);
			} while(DateTime.Compare(day, this.InputDeparture.SelectedDate.Value) < 0);

			this.core.DisplayRates(new RequestSheet() {
				Channels = new List<int> { 0 },
				RoomTypes = new List<int> { ((ComboboxItem)this.InputRooms.SelectedItem).ID },
				Rates = new List<int> { },
				Dates = stays,
				Packages = new List<int> { }
			});
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void AgesValidationTextBox(object sender, TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9,]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void InputNumber_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			try {
				TextBox textBox = (TextBox) sender;
				int value = Convert.ToInt32(textBox.Text);

				switch(e.Key) {
					case Key.Down:
					textBox.Text = (value - 1).ToString();
					break;
					case Key.Up:
					textBox.Text = (value + 1).ToString();
					break;
				}
			} catch(Exception a) {
				Console.WriteLine(a.StackTrace);
			}
		}
	}
}
