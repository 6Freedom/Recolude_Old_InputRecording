using UnityEngine;


namespace RecordAndPlay.Record
{

    /// <summary>
    /// A subject that uses a game object as it's foundation for values to be recorded.
    /// </summary>
    public class GameObjectSubject : ISubject
    {

        private GameObject obj;

        /// <summary>
        /// Builders a subject around the gameobject
        /// </summary>
        /// <param name="obj">The object to capture values from</param>
        public GameObjectSubject(GameObject obj)
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException("requires non-null obj as a subject");
            }
            this.obj = obj;
        }

        /// <summary>
        /// Builders a subject around the gameobject
        /// </summary>
        /// <param name="obj">The object to capture values from</param>
        public GameObjectSubject(Transform obj) : this(obj.gameObject)
        {
        }

        /// <summary>
        /// The Gameobject that this subject records.
        /// </summary>
        /// <returns>The Gameobject that this subject records.</returns>
        public GameObject GetGameObject()
        {
            return this.obj;
        }

        public int GetID()
        {
            return obj.GetInstanceID();
        }

        public string GetName()
        {
            return obj.name;
        }

        public Vector3 GetPosition()
        {
            return obj.transform.position;
        }

        public Vector3 GetRotation()
        {
            return obj.transform.rotation.eulerAngles;
        }

    }

}