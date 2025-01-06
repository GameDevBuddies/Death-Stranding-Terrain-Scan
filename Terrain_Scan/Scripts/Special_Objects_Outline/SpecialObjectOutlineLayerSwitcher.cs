using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Class that listens to the terrain scan start event and switches the 
    /// layer of the object from normal to outline if the object is inside the terrain scan
    /// range and angle threshold.
    /// </summary>
    public class SpecialObjectOutlineLayerSwitcher: MonoBehaviour
    {
        [SerializeField] private int _normalLayer = 0;
        [SerializeField] private int _outlineLayer = 0;

        private TerrainScan _terrainScan = null;

        private void Start()
        {
            _terrainScan = TerrainScan.Instance;
            _terrainScan.OnTerrainScanStart += CheckForLayerSwitch;
            _terrainScan.OnTerrainScanEnd += RevertToNormalLayer;

            SetLayer(transform, _normalLayer);
        }

        private void OnDestroy()
        {
            if(_terrainScan == null)
            {
                return;
            }

            _terrainScan.OnTerrainScanStart -= CheckForLayerSwitch;
            _terrainScan.OnTerrainScanEnd -= RevertToNormalLayer;
        }

        private void CheckForLayerSwitch(TerrainScanInfo _)
        {
            if(_terrainScan == null)
            {
                return;
            }

            if(_terrainScan.IsInsideTerrainScanActivationRange(transform.position) &&
               _terrainScan.IsInsideTerrainScanAngle(transform.position))
            {
                SetLayer(transform, _outlineLayer);
            }
            else
            {
                SetLayer(transform, _normalLayer);
            }
        }

        private void RevertToNormalLayer()
        {
            SetLayer(transform, _normalLayer);
        }

        private void SetLayer(Transform currentTransform, int layer)
        {
            currentTransform.gameObject.layer = layer;
            foreach(Transform child in currentTransform)
            {
                SetLayer(child, layer);
            }
        }
    }
}