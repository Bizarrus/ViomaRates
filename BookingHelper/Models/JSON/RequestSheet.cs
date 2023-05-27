using System.Collections.Generic;

namespace BookingHelper.Models.JSON {
	public class RequestSheet {
		public IList<int> Channels {
			get; set;
		}
		public IList<int> RoomTypes {
			get; set;
		}
		public IList<int> Rates {
			get; set;
		}
		public IList<int> Dates {
			get; set;
		}
		public IList<int> Packages {
			get; set;
		}
	}
}
