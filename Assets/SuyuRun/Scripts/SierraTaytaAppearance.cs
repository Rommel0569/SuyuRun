using UnityEngine;
namespace SuyuRun
{
    // One shared material and property block: no texture copies or sprite changes.
    public static class SierraTaytaAppearance
    {
        static Material matte;static MaterialPropertyBlock properties;
        public static Material Material
        {
            get
            {
                if(!matte)
                {
                    var shader=Resources.Load<Shader>("SierraTaytaMatte")??Shader.Find("SuyuRun/SierraTaytaMatte");
                    if(shader)matte=new Material(shader){name="Sierra Tayta matte palette"};
                }
                return matte;
            }
        }
        public static void Reset(SpriteRenderer renderer)
        {Set(renderer,0);}
        public static void Apply(SpriteRenderer renderer)
        {
            if(!renderer||!Material)return;
            renderer.sharedMaterial=Material;Set(renderer,1);
        }
        static void Set(SpriteRenderer renderer,float value)
        {
            if(properties==null)properties=new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);properties.SetFloat("_SierraMatte",value);renderer.SetPropertyBlock(properties);
        }
    }
}
