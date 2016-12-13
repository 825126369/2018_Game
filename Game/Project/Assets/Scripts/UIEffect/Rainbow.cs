using UnityEngine;
using System.Collections;
using xk_System.Debug;
using UnityEngine.UI;
using System.Collections.Generic;

namespace xk_System.View.Module
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class Rainbow : BaseMeshEffect
    {
        private Color32[] colors;
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }
            List<UIVertex> vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);
            int count = vertices.Count;  
            colors = new Color32[count / 6];
            for (int i = 0; i < colors.Length; i++)
            {
                //随机颜色
                float r = Random.value;
                float g = Random.value;
                float b = Random.value;
                colors[i] = new Color(r, g, b);
            }
            UIVertex v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                v.color = colors[i / 4];
                vh.SetUIVertex(v, i);
            }
        }
    }
}