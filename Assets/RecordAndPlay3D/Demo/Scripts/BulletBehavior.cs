using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Record;


namespace RecordAndPlay.Demo
{
    public class BulletBehavior : MonoBehaviour
    {
        SubjectBehavior subject;

        public static BulletBehavior Build(SubjectBehavior subject)
        {
            BulletBehavior bullet = subject.gameObject.AddComponent<BulletBehavior>();
            bullet.subject = subject;
            return bullet;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                subject.CaptureCustomEvent("Collision", new Dictionary<string, string>()
                {
                    {"x" , transform.position.x.ToString()},
                    {"y" , transform.position.y.ToString()},
                    {"z" , transform.position.z.ToString()}
                });
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.transform.name == "Cube")
            {
                
                var contactPoint = collision.contacts[0].point;
                subject.CaptureCustomEvent("Collision", new Dictionary<string, string>()
                {
                    {"x" , contactPoint.x.ToString()},
                    {"y" , contactPoint.y.ToString()},
                    {"z" , contactPoint.z.ToString()}
                });
            }
        }

    }

}