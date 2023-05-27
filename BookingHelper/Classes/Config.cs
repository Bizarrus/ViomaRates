using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BookingHelper.Classes {
	public sealed class Config {
		private static Dictionary<string, string> temp = new Dictionary<string, string>();

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		public static string Get(string section, string key) {
			var output = new StringBuilder(255);
			GetPrivateProfileString(section, key, "", output, 255, new FileInfo("Settings.ini").FullName);

			if(output.Length > 0) {
				return output.ToString();
			}

			if(Config.temp.ContainsKey(section + "." + key)) {
				string result;

				if(Config.temp.TryGetValue(section + "." + key, out result)) {
					return result;
				}
			}

			return "";
		}

		public static void SetTemporary(string section, string key, string value) {
			if(Config.temp.ContainsKey(section + "." + key)) {
				Config.temp.Remove(section + "." + key);
			}
			
			Config.temp.Add(section + "." + key, value);
		}

		public static void Set(string section, string key, string value) {
			WritePrivateProfileString(section, key, value, new FileInfo("Settings.ini").FullName);
		}

		public static void Remove(string section, string key) {
			Set(key, null, section);
		}

		public static void RemoveSection(string section, string key) {
			Set(null, null, section);
		}
		public static bool Exists(string section, string key) {
			return Get(section, key).Length > 0;
		}
	}
}
