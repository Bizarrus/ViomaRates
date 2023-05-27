namespace BookingHelper.Models.JSON {
	public class User {
		public int ID {
			get; set;
		}
		public int Company {
			get; set;
		}
		public string Name {
			get; set;
		}
		public string FullName {
			get; set;
		}
		public string EMail {
			get; set;
		}
		public string Description {
			get; set;
		}
		public bool Disabled {
			get; set;
		}
		public bool Inactive {
			get; set;
		}
		public bool Deleted {
			get; set;
		}
		public bool PassForceChange {
			get; set;
		}
		public int Interface {
			get; set;
		}
		public int LanguageData {
			get; set;
		}
		public int LanguageInterface {
			get; set;
		}
		public string Data {
			get; set;
		}
	}
}
