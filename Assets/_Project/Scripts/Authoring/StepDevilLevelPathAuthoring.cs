using System;
using UnityEngine;

namespace StepDevil
{
    /// <summary>
    /// Place in the scene to visualize each fork step of a level as gizmos (see custom inspector / Scene view).
    /// Set <see cref="LevelId"/> to the level number (1–12) and optionally assign one offset per fork step (local space).
    /// </summary>
    public sealed class StepDevilLevelPathAuthoring : MonoBehaviour
    {
        [SerializeField] int _levelId = 1;
        [SerializeField] Vector3[] _forkOffsets = Array.Empty<Vector3>();
        [SerializeField] Color _gizmoColor = new Color(1f, 0.3f, 0.4f, 1f);

        public int LevelId => _levelId;
        public Color GizmoColor => _gizmoColor;

        public Vector3 GetForkPosition(int forkIndex)
        {
            if (_forkOffsets != null && forkIndex >= 0 && forkIndex < _forkOffsets.Length)
                return transform.TransformPoint(_forkOffsets[forkIndex]);

            return transform.TransformPoint(Vector3.right * (forkIndex * 1.5f));
        }
    }
}
