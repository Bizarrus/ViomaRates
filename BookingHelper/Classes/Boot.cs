using System;

namespace BookingHelper.Classes {
	class Boot {
		public static Core core { get; private set; } = null;

		[STAThread]
		public static void Main(String[] args) {
			Config.Get("Section", "Key");

			core = new Core();
			core.Start();
		}
	}
}
