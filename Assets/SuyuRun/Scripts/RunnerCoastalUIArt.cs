using UnityEngine;

namespace SuyuRun
{
    // Loads the approved pixel-art UI pieces (HUD, inicio, selector, museo, tienda) and draws them
    // through OnGUI - the game already renders its whole UI via OnGUI (see DrawTitleScreen/
    // DrawMuseum/DrawCompactCoastHud), so this keeps that architecture and swaps the flat colored
    // boxes for the approved textures instead of migrating to uGUI mid-project.
    public sealed partial class RunnerPrototype
    {
        Texture2D texHud, texInicio, texSelector, texMuseoPared, texMuseoVitrina, texMuseoPanel, texTienda, texResultados;
        Sprite[] hudSprites;

        void LoadCoastalUIArt()
        {
            texHud=Resources.Load<Texture2D>("costa_hud");
            texInicio=Resources.Load<Texture2D>("costa_inicio");
            texSelector=Resources.Load<Texture2D>("costa_selector");
            texMuseoPared=Resources.Load<Texture2D>("costa_museo_pared");
            texMuseoVitrina=Resources.Load<Texture2D>("costa_museo_vitrina");
            texMuseoPanel=Resources.Load<Texture2D>("costa_museo_panel");
            texTienda=Resources.Load<Texture2D>("costa_tienda");
            texResultados=Resources.Load<Texture2D>("costa_resultados");
            hudSprites=Resources.LoadAll<Sprite>("costa_hud");
        }
        Sprite HudSprite(string name){if(hudSprites==null)return null;foreach(var s in hudSprites)if(s.name==name)return s;return null;}

        // Draws a full Texture2D stretched into `rect`, Point-filtered (no smoothing) - used for
        // whole-screen backgrounds and single-piece art (inicio, selector, museo panels, tienda).
        void DrawTex(Rect rect,Texture2D tex){if(!tex)return;GUI.DrawTexture(rect,tex,ScaleMode.StretchToFill);}

        // Draws one sub-sprite of an atlas (costa_hud.png) stretched into `rect`, converting the
        // sprite's pixel rect to normalized UV since GUI.DrawTexture can't take a Sprite directly.
        void DrawSprite(Rect rect,Sprite sprite)
        {
            if(!sprite)return;
            var tex=sprite.texture;var r=sprite.textureRect;
            var uv=new Rect(r.x/tex.width,r.y/tex.height,r.width/tex.width,r.height/tex.height);
            GUI.DrawTextureWithTexCoords(rect,tex,uv);
        }
        // Tiles a sub-sprite horizontally across `rect` at native pixel size (x4 GUI scale) instead
        // of stretching it, so repeating textures (bar fills, wall bricks) don't smear.
        void DrawSpriteTiled(Rect rect,Sprite sprite,float pxScale)
        {
            if(!sprite)return;
            float w=sprite.rect.width*pxScale;
            for(float x=rect.x;x<rect.x+rect.width;x+=w)
            {
                float remaining=Mathf.Min(w,rect.x+rect.width-x);
                var sub=new Rect(x,rect.y,remaining,rect.height);
                var tex=sprite.texture;var r=sprite.textureRect;
                var uv=new Rect(r.x/tex.width,r.y/tex.height,(r.width/tex.width)*(remaining/w),r.height/tex.height);
                GUI.DrawTextureWithTexCoords(sub,tex,uv);
            }
        }
    }
}
