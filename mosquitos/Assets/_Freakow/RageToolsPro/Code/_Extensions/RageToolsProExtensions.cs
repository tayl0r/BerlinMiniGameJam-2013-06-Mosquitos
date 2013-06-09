using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Rage {
	internal static class RageToolsProExtensions {

// 		/// <summary> Updates the Vertex Density on every element of a path list.</summary>
// 		/// <param name="list"></param>
// 		/// <param name="density">Absolute density or additive factor, per path</param>
// 		public static void UpdateDensity (this List<RageGroupElement> list, int density) {
// 			if (list == null) return;
// 			foreach (RageGroupElement rageGroupItem in list) {
// 				ISpline path = rageGroupItem.Spline;
// 				if (!path.IsVisible()) continue;
// 				if (path.VertexDensity == density) continue;
// 
// 				path.VertexDensity = density;
// 			}
// 		}

		/// <summary> Returns found frame, or null if no frame found </summary>
		public static RageSpriteFrame ContainsFrame(this List<RageSpriteFrame> frames, GameObject frameObject) {
			foreach (RageSpriteFrame thisFrame in frames) {
				if (thisFrame.gO == frameObject)
					return thisFrame;
			}
			return null;
		}

		public static RageSpriteFrame GetFrame (this List<RageSpriteFrame> Frames, string frameName) {
			return Frames.FirstOrDefault (thisFrame => thisFrame.gO.name == frameName);
		}
	}
}
