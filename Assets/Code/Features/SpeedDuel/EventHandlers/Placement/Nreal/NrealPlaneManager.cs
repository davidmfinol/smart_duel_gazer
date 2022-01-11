using System.Collections.Generic;
using NRKernal;
using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers.Placement.Nreal
{
    /// <summary> There is a PlaneDetector in the NRSDK samples, but it does
    /// not store the instantiated plane object. This class does what the PlaneDetector
    /// does and more, so that we can manipulate the plane objects. </summary>
    public class NrealPlaneManager : MonoBehaviour
    {
        public GameObject detectedPlanePrefab;
        
        private readonly List<NRTrackablePlane> _newPlanes = new List<NRTrackablePlane>();
        private readonly List<GameObject> _planeObjects = new List<GameObject>();

        public void Update()
        {
            NRFrame.GetTrackables(_newPlanes, NRTrackableQueryFilter.New);
            foreach (var newPlane in _newPlanes)
            {
                var planeObject = Instantiate(detectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
                planeObject.GetComponent<NRTrackableBehaviour>().Initialize(newPlane);
                
                _planeObjects.Add(planeObject);
            }
        }
        
        public void SetPlaneObjectsActive(bool active)
        {
            foreach (var planeObject in _planeObjects)
            {
                planeObject.SetActive(active);
            }
        }
    }
}