using System.Collections.Generic;

//namespace Rage {

	public static class StringExtension {

		private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		/// <summary> Removes any trailing alphabetic (A-Z) characters from the string </summary>
		/// <param name="stringToTrim"> String to be trimmed </param>
		/// <returns></returns>
		public static string TrimTrailingAlphas(this string stringToTrim) {
			int trimLength = 0;
			for (int i = stringToTrim.Length - 1; i >= 0; i--) {
				char letter = stringToTrim[i];
				// Proceeds until it finds an alphabetic or percentage char
				if(!(letter.IsLetter() || letter == '%')) break;
				trimLength++;
			}

			if (trimLength > 0)
				stringToTrim = stringToTrim.Substring(0, stringToTrim.Length - trimLength);

			return stringToTrim;
		}

		public static string[] RemoveEmptyEntries(this string[] stringArray) {
			var newArray = new List<string>();

			foreach(string t in stringArray) {
				if(t == null) continue;
				if(t == "") continue;
				newArray.Add(t);
			}

			return newArray.ToArray();
		}

		public static bool IsLetter(this char value) { return Letters.Contains("" + value); }
		public static float SvgToFloat(this string svgValue) {
			float value;
			float.TryParse(svgValue.TrimTrailingAlphas(), out value);
			return value;
		}

		public static string Pad(this string text, int alignment, int displaySize) {
			switch(alignment) {
				case 0: return text.PadRight(displaySize); //Left
				case 1: return text.PadRight(displaySize); //Center
				case 2: return text.PadLeft(displaySize); //Right
				default: return text;
			}
		}

	}
//}
