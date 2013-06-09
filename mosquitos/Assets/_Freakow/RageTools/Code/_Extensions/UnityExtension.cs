using UnityEngine;

//namespace Rage {

    public static class UnityExtension {

        public static float GetCameraArea(this Camera camera) {
            Vector3 bottomleft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            Vector3 topright = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane));
            return (topright.x - bottomleft.x) * (topright.y - bottomleft.y);
        }

        /// <summary> Returns the first child of a object with a certain name </summary>
        /// <param name="parentObj"> Parent gO whose children will be searched </param>
        /// <param name="searchString"> Searched Name </param>
        public static GameObject GetChildByName(this GameObject parentObj, string searchString) {
            foreach (Transform child in parentObj.transform) {
                if (!child.name.Equals(searchString)) continue;
                return child.gameObject;
            }
            return null;
        }
    }
//}
