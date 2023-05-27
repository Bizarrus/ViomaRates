using System;

namespace BookingHelper.Models {
	public class Catering {
		private int id;
		private String name;
		private String text;

		public Catering(int id, String name, String text) {
			this.id = id;
			this.name = name;
			this.text = text;
		}

		public int GetID() {
			return this.id;
		}

		public String GetName() {
			return this.name;
		}

		public String GetText() {
			return this.text;
		}

		public override String ToString() {
			return string.Format("[Catering Id={0}, Name={1}, Text={2}]", this.id, this.name, this.text);
		}
	}
}
