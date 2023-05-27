using System;
using System.Collections.Generic;

namespace BookingHelper.Models.JSON {
	public class SheetEntry {
		public string RecordID {
			get; set;
		}
		public int HotelID {
			get; set;
		}
		public int Date {
			get; set;
		}
		public int RoomType {
			get; set;
		}
		public int Rate {
			get; set;
		}
		public int Channel {
			get; set;
		}
		public int Package {
			get; set;
		}
		public string DateChanged {
			get; set;
		}
		public int LayerType {
			get; set;
		}

#nullable enable
		public Dictionary<string, Dictionary<string, object>>? RoomPrices {
			get; set;
		}
		public Dictionary<string, Dictionary<string, object>>? ChildrenPrices {
			get; set;
		}
		public Dictionary<string, int>? UnitsAvail {
			get; set;
		}
		public Dictionary<string, int>? UnitsAvailGapBefore {
			get; set;
		}
		public Dictionary<string, int>? UnitsAvailGapAfter {
			get; set;
		}
		public Dictionary<string, int>? RestrictionQuota {
			get; set;
		}
		public Dictionary<string, object>? RestrictionClosed {
			get; set;
		}
		public Dictionary<string, object>? RestrictionMinLOS {
			get; set;
		}
		public Dictionary<string, object>? RestrictionMaxLOS {
			get; set;
		}
	}
}
