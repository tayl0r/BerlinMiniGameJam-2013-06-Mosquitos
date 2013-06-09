using UnityEngine;
using System.Collections.Generic;

//namespace Rage {

	public static class ColorExtension {

		private static Dictionary<string, string> _keywords;
		public static string KeywordToHex(this string keyword) {
			InitializeKeywordsIfNecessary();
			return _keywords[keyword];
		}

		public static Color ToColor(this string color) {
			if(color == "") return Color.black;
			// If it's a shorthand color, duplicate each char
			if (color.Length==3) {
				var cc = color.ToCharArray();
				color = (cc[0]+"") + (cc[0]+"") + (cc[1]+"") + (cc[1]+"") + (cc[2]+"") + (cc[2]+"");
			}
			float red = (float)(GetInt(color[0]) * 16 + GetInt(color[1])) / 255;
			float green = (float)(GetInt(color[2]) * 16 + GetInt(color[3])) / 255;
			float blue = (float)(GetInt(color[4]) * 16 + GetInt(color[5])) / 255;
			return new Color { r = red, g = green, b = blue, a = 1 };
		}

		private static int GetInt(char hexChar) {
			switch(hexChar.ToString().ToUpper()) {
				case "0": return 0;
				case "1": return 1;
				case "2": return 2;
				case "3": return 3;
				case "4": return 4;
				case "5": return 5;
				case "6": return 6;
				case "7": return 7;
				case "8": return 8;
				case "9": return 9;
				case "A": return 10;
				case "B": return 11;
				case "C": return 12;
				case "D": return 13;
				case "E": return 14;
				case "F": return 15;
			}
			return 0;
		}

		/// <summary> "Validates" any letter but the exponential tag ('e') as a command </summary>
		public static bool IsPathCommand(this char letter) {
			return (char.IsLetter(letter) && letter != 'e' && letter != 'E');
		}

		private static void InitializeKeywordsIfNecessary() {
			if(_keywords != null) return;

			_keywords = new Dictionary<string, string>
							{
				{"aliceblue", "F0F8FF"},
				{"antiquewhite", "FAEBD7"},
				{"aqua", "00FFFF"},
				{"aquamarine", "7FFFD4"},
				{"azure", "F0FFFF"},
				{"beige", "F5F5DC"},
				{"bisque", "FFE4C4"},
				{"black", "000000"},
				{"blanchedalmond", "FFEBCD"},
				{"blue", "0000FF"},
				{"blueviolet", "8A2BE2"},
				{"brown", "A52A2A"},
				{"burlywood", "DEB887"},
				{"cadetblue", "5F9EA0"},
				{"chartreuse", "7FFF00"},
				{"chocolate", "D2691E"},
				{"coral", "FF7F50"},
				{"cornflowerblue", "6495ED"},
				{"cornsilk", "FFF8DC"},
				{"crimson", "DC143C"},
				{"cyan", "00FFFF"},
				{"darkblue", "00008B"},
				{"darkcyan", "008B8B"},
				{"darkgoldenrod", "B8860B"},
				{"darkgray", "A9A9A9"},
				{"darkgreen", "006400"},
				{"darkgrey", "A9A9A9"},
				{"darkkhaki", "BDB76B"},
				{"darkmagenta", "8B008B"},
				{"darkolivegreen", "556B2F"},
				{"darkorange", "FF8C00"},
				{"darkorchid", "9932CC"},
				{"darkred", "8B0000"},
				{"darksalmon", "E9967A"},
				{"darkseagreen", "8FBC8F"},
				{"darkslateblue", "483D8B"},
				{"darkslategray", "2F4F4F"},
				{"darkslategrey", "2F4F4F"},
				{"darkturquoise", "00CED1"},
				{"darkviolet", "9400D3"},
				{"deeppink", "FF1493"},
				{"deepskyblue", "00BFFF"},
				{"dimgray", "696969"},
				{"dimgrey", "696969"},
				{"dodgerblue", "1E90FF"},
				{"firebrick", "B22222"},
				{"floralwhite", "FFFAF0"},
				{"forestgreen", "228B22"},
				{"fuchsia", "FF00FF"},
				{"gainsboro", "DCDCDC"},
				{"ghostwhite", "F8F8FF"},
				{"gold", "FFD700"},
				{"goldenrod", "DAA520"},
				{"gray", "808080"},
				{"grey", "808080"},
				{"green", "008000"},
				{"greenyellow", "ADFF2F"},
				{"honeydew", "F0FFF0"},
				{"hotpink", "FF69B4"},
				{"indianred", "CD5C5C"},
				{"indigo", "4B0082"},
				{"ivory", "FFFFF0"},
				{"khaki", "F0E68C"},
				{"lavender", "E6E6FA"},
				{"lavenderblush", "FFF0F5"},
				{"lawngreen", "7CFC00"},
				{"lemonchiffon", "FFFACD"},
				{"lightblue", "ADD8E6"},
				{"lightcoral", "F08080"},
				{"lightcyan", "E0FFFF"},
				{"lightgoldenrodyellow", "FAFAD2"},
				{"lightgray", "D3D3D3"},
				{"lightgreen", "90EE90"},
				{"lightgrey", "D3D3D3"},
				{"lightpink", "FFB6C1"},
				{"lightsalmon", "FFA07A"},
				{"lightseagreen", "20B2AA"},
				{"lightskyblue", "87CEFA"},
				{"lightslategray", "778899"},
				{"lightslategrey", "778899"},
				{"lightsteelblue", "B0C4DE"},
				{"lightyellow", "FFFFE0"},
				{"lime", "00FF00"},
				{"limegreen", "32CD32"},
				{"linen", "FAF0E6"},
				{"magenta", "FF00FF"},
				{"maroon", "800000"},
				{"mediumaquamarine", "66CDAA"},
				{"mediumblue", "0000CD"},
				{"mediumorchid", "BA55D3"},
				{"mediumpurple", "9370DB"},
				{"mediumseagreen", "3CB371"},
				{"mediumslateblue", "7B68EE"},
				{"mediumspringgreen", "00FA9A"},
				{"mediumturquoise", "48D1CC"},
				{"mediumvioletred", "C71585"},
				{"midnightblue", "191970"},
				{"mintcream", "F5FFFA"},
				{"mistyrose", "FFE4E1"},
				{"moccasin", "FFE4B5"},
				{"navajowhite", "FFDEAD"},
				{"navy", "000080"},
				{"oldlace", "FDF5E6"},
				{"olive", "808000"},
				{"olivedrab", "6B8E23"},
				{"orange", "FFA500"},
				{"orangered", "FF4500"},
				{"orchid", "DA70D6"},
				{"palegoldenrod", "EEE8AA"},
				{"palegreen", "98FB98"},
				{"paleturquoise", "AFEEEE"},
				{"palevioletred", "DB7093"},
				{"papayawhip", "FFEFD5"},
				{"peachpuff", "FFDAB9"},
				{"peru", "CD853F"},
				{"pink", "FFC0CB"},
				{"plum", "DDA0DD"},
				{"powderblue", "B0E0E6"},
				{"purple", "800080"},
				{"red", "FF0000"},
				{"rosybrown", "BC8F8F"},
				{"royalblue", "4169E1"},
				{"saddlebrown", "8B4513"},
				{"salmon", "FA8072"},
				{"sandybrown", "F4A460"},
				{"seagreen", "2E8B57"},
				{"seashell", "FFF5EE"},
				{"sienna", "A0522D"},
				{"silver", "C0C0C0"},
				{"skyblue", "87CEEB"},
				{"slateblue", "6A5ACD"},
				{"slategray", "708090"},
				{"slategrey", "708090"},
				{"snow", "FFFAFA"},
				{"springgreen", "00FF7F"},
				{"steelblue", "4682B4"},
				{"tan", "D2B48C"},
				{"teal", "008080"},
				{"thistle", "D8BFD8"},
				{"tomato", "FF6347"},
				{"turquoise", "40E0D0"},
				{"violet", "EE82EE"},
				{"wheat", "F5DEB3"},
				{"white", "FFFFFF"},
				{"whitesmoke", "F5F5F5"},
				{"yellow", "FFFF00"},
				{"yellowgreen", "9ACD32"}
			};
		}

		public static float CalcTheta(Vector2 point1, Vector2 point2) {
			float theta;
			if(point2.x - point1.x == 0)
				theta = point2.y > point1.y ? 0 : Mathf.PI;
			else {
				theta = Mathf.Atan((point2.y - point1.y) / (point2.x - point1.x));
				if(point2.x > point1.x)
					theta = Mathf.PI / 2.0f - theta;
				else
					theta = Mathf.PI * 1.5f - theta;
			}
			return theta * 180 / Mathf.PI;
		}
	}
//}
