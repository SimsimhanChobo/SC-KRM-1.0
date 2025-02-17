using SCKRM.UI;
using UnityEditor;

namespace SCKRM.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BetterContentSizeFitter))]
    public class BetterContentSizeFitterEditor : UIEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UseProperty("_target");

            Space();

            UseProperty("_xSize", "X 크기 변경");
            UseProperty("_ySize", "Y 크기 변경");

            Space();

            UseProperty("_offset");
            UseProperty("_minSize");
            UseProperty("_maxSize");
        }
    }
}