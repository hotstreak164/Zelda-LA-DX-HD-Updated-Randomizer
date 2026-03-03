using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Things;
using ProjectZ.InGame.Map;

namespace ProjectZ.InGame.Things
{
    public class CameraField
    {
        public List<GameObject> GameObjectList = new List<GameObject>();
        public List<ObjCameraField> CameraFieldList = new List<ObjCameraField>();

        public Vector2 CameraFieldCoords;

        public void AddToList(ObjCameraField fieldCenter)
        {
            CameraFieldList.Add(fieldCenter);
        }

        public void SetClosestCoords()
        {
            // If the list is empty, there is no camera coords to obtain.
            if (CameraFieldList.Count <= 0)
            {
                CameraFieldCoords = Vector2.Zero;
                return;
            }
            // Get Link's position to determine the closest camera.
            Vector2 playerPos = MapManager.ObjLink.CenterPosition.Position;
            ObjCameraField closestCam = null;
            float closestDist = float.MaxValue;

            // Loop through all the camera objects found.
            foreach (ObjCameraField fieldCenter in CameraFieldList)
            {
                // If there are multiple camera objects, find the closest camera to Link.
                float dist = Vector2.Distance(playerPos, fieldCenter.EntityPosition.Position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestCam = fieldCenter;
                }
            }
            // Return the coordinates of the closest camera.
            CameraFieldCoords = new Vector2(closestCam.EntityPosition.X, closestCam.EntityPosition.Y);
        }

        public void FindClosestCoords()
        {
            // Grab objects with a certain range.
            GameObjectList.Clear();
            GameObjectList = MapManager.ObjLink.Map.Objects.GetObjects((int)MapManager.ObjLink.CenterPosition.X - 160, (int)MapManager.ObjLink.CenterPosition.Y -100, 320, 200);

            // Loop through the game objects.
            foreach (var gameObject in GameObjectList)
            {
                // Find camera field objects.
                if (gameObject is ObjCameraField camField)
                {
                    // Add the camera object to the list.
                    AddToList(camField);
                }
                // Set the closest camera to 
                SetClosestCoords();
            }
        }

        public void ClearList()
        {
            // Clear the camera field object list and clear properties.
            CameraFieldList.Clear();
            CameraFieldCoords = Vector2.Zero;
        }
    }
}
