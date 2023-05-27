using BookingHelper.Classes;
using BookingHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace BookingHelper.Networking {
	public class Feed {
		List<Room> rooms            = new List<Room>();
		List<Catering> caterings    = new List<Catering>();
		List<Children> childrens    = new List<Children>();

		public void Connect(Action action) {
			if(!Config.Exists("Hotel", "ID")) {
				MessageBox.Show($"Settings.ini: The Value of <ID> on <Hotel> section is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if(!Config.Exists("Endpoint", "ClientID")) {
				MessageBox.Show($"Settings.ini: The Value of <ClientID> on <Endpoint> section is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
			queryString.Add("hotel", Config.Get("Hotel", "ID"));
			queryString.Add("data_gallery", "0");
			queryString.Add("data_programs", "0");
			queryString.Add("data_packages", "0");
			queryString.Add("data_pension_types", "1");
			queryString.Add("data_roomtypes", "1");
			queryString.Add("data_rooms_alloc", "1");

			XmlDocument doc1 = new XmlDocument();
			doc1.Load("https://" + Config.Get("Endpoint", "ClientID") + ".viomassl.com/xml.php?" + queryString.ToString());
			XmlElement root = doc1.DocumentElement;

			// Find catering types
			foreach(XmlNode node in root.SelectNodes("/hotels/hotel/pension-types/pension-type")) {
				string hrt_id   = node["data"]["hpt_id"].InnerText;
				string hrt_name = node["data"]["hpt_name_internal"].InnerText;
				string hrt_text = node["data"]["hpt_meal_plan_hcatering_name"].InnerText;
				Catering type   = new Catering(Int32.Parse(hrt_id), hrt_name, hrt_text);
				caterings.Add(type);
			}

			// Find room types
			foreach(XmlNode node in root.SelectNodes("/hotels/hotel/roomtypes/roomtype")) {
				string hrt_id   = node["data"]["hrt_id"].InnerText;
				string hrt_name = node["data"]["hrt_name"].InnerText;
				string hrt_text = node["data"]["hrt_name_internal"].InnerText;
				Room room       = new Room(Int32.Parse(hrt_id), hrt_name, hrt_text);
				rooms.Add(room);
			}

			// Find children packages
			foreach(XmlNode node in root.SelectNodes("/hotels/hotel/childrens/children")) {
				string hrt_id         = node.Attributes["id"].InnerText;
				string from           = node.Attributes["age-from"].InnerText;
				string to             = node.Attributes["age-to"].InnerText;
				Children children     = new Children(Int32.Parse(hrt_id), Int32.Parse(from), Int32.Parse(to));
				this.childrens.Add(children);
			}

			// Run after fetching data
			action.Invoke();
		}

		public List<Room> GetRooms() {
			return this.rooms;
		}

		public List<Catering> GetCaterings() {
			return this.caterings;
		}

		public List<Children> GetChildrens() {
			return this.childrens;
		}
	}
}
