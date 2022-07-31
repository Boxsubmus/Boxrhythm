// This isn't clean code, this is just an example of what you can do with Boxrhythm.
// Not what you're "SUPPOSED" to do.

using UnityEngine;

namespace Boxsubmus.Boxrhythm.Tests
{
    public class TestingFunctions : MonoBehaviour
    {
        public Conductor conductor;

        private void Update()
        {
            transform.position = new Vector3(Mathf.Lerp(-6, 6, conductor.GetLoopPositionFromBeat(0, 2)), 2);
        }
    }
}
