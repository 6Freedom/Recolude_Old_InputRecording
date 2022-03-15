using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Playback;

namespace Recolude
{

    /// <summary>
    /// Mimics what [Recolude's webplayer](https://app.recolude.com/) does. 
    /// Read more about [Actor Builders](xref:RecordAndPlay.Playback.IActorBuilder) 
    /// to learn how to build your own. 
    /// </summary>
    public class RecoludeActorBuilder : IActorBuilder
    {

        private static Vector3 ParseVectorString(string vec3string)
        {
            var components = vec3string.Split(',');
            if (components.Length != 3)
            {
                return new Vector3(1, 1, 1);
            }

            var x = float.Parse(components[0]);
            var y = float.Parse(components[1]);
            var z = float.Parse(components[2]);

            return new Vector3(x, y, z);
        }

        public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
        {
            GameObject gameObject;

            if (metadata.ContainsKey("recolude-geom"))
            {
                switch (metadata["recolude-geom"].ToLower())
                {
                    case "cube":
                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;

                    case "sphere":
                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;

                    case "plane":
                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        break;

                    case "capsule":
                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        break;

                    default:
                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                }
            }
            else
            {
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            // Do scale
            if (metadata.ContainsKey("recolude-scale"))
            {
                gameObject.transform.localScale = ParseVectorString(metadata["recolude-scale"]);
            }

            // Do color
            if (metadata.ContainsKey("recolude-color"))
            {
                Color color = Color.white;
                ColorUtility.TryParseHtmlString(metadata["recolude-color"], out color);
                gameObject.GetComponent<MeshRenderer>().material.color = color;
            }

            // Do offset
            if (metadata.ContainsKey("recolude-offset"))
            {
                gameObject.transform.position += ParseVectorString(metadata["recolude-offset"]);
                GameObject parent = new GameObject();
                gameObject.transform.SetParent(parent.transform);
                gameObject = parent;
            }

            return new Actor(gameObject);
        }
    
    }

}