using Newtonsoft.Json;
using System.Collections.Generic;

namespace BookingHelper.Models.JSON {
	public class LoginResult {
		public User User {
			get; set;
		}
		public string Token {
			get; set;
		}
		public string Error {
			get; set;
		}
		public IList<IList<int>> Permissions {
			get; set;
		}
	}
}
