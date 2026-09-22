using UnityEngine;

namespace SuyuRun
{
    // Coin-funded upgrades (ART_BIBLE.md section 10: "las monedas se gastan en mejoras"). Only
    // RÁFAGA duration and a coin magnet exist - no vida upgrade, since Costa has no vida stat
    // (confirmed earlier; that starts in Selva). Persisted in `bank`, same currency as everywhere
    // else, so upgrades apply across replays.
    public sealed partial class RunnerPrototype
    {
        const int MaxUpgradeLevel=3;
        int upgradeRafaga,upgradeIman;
        void LoadUpgrades()
        {
            upgradeRafaga=PlayerPrefs.GetInt("SuyuRun.Upgrade.Rafaga",0);
            upgradeIman=PlayerPrefs.GetInt("SuyuRun.Upgrade.Iman",0);
        }
        int UpgradeCost(int currentLevel)=>(currentLevel+1)*20;
        bool BuyUpgrade(ref int level,string prefKey)
        {
            if(level>=MaxUpgradeLevel)return false;
            int cost=UpgradeCost(level);
            if(bank<cost)return false;
            bank-=cost;level++;
            PlayerPrefs.SetInt(prefKey,level);PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",bank);PlayerPrefs.Save();
            Beep(900,.12f);
            return true;
        }
        void DrawShop()
        {
            if(!texTienda){if(GUI.Button(new Rect(40,40,260,44),"VOLVER",button))ReturnToMenu();return;}
            GUI.Label(new Rect(40,20,1200,60),"TIENDA",title);
            GUI.Label(new Rect(40,74,600,36),$"MONEDAS GUARDADAS: {bank}",text);
            // costa_tienda.png is 280x180 native - keep that aspect, don't stretch to the full 1280 width.
            float scale=2.6f;
            Rect panelRect=new Rect(40,130,280*scale,180*scale);
            DrawTex(panelRect,texTienda);
            int cardW=(int)(78*scale),cardH=(int)(120*scale),gap=(int)(6*scale);
            int x0=(int)(panelRect.x+10*scale),y0=(int)(panelRect.y+34*scale);
            DrawUpgradeCard(x0,y0,cardW,cardH,"RÁFAGA",upgradeRafaga,()=>BuyUpgrade(ref upgradeRafaga,"SuyuRun.Upgrade.Rafaga"));
            DrawUpgradeCard(x0+cardW+gap,y0,cardW,cardH,"IMÁN DE MONEDAS",upgradeIman,()=>BuyUpgrade(ref upgradeIman,"SuyuRun.Upgrade.Iman"));
            GUI.Label(new Rect(x0+2*(cardW+gap),y0+cardH-30,cardW,60),"(bloqueado)\nsin stat de vida\nen Costa",new GUIStyle(small){alignment=TextAnchor.MiddleCenter,wordWrap=true});
            if(GUI.Button(new Rect(40,650,260,44),"VOLVER AL INICIO",button))ReturnToMenu();
        }
        delegate bool BuyFn();
        void DrawUpgradeCard(int x,int y,int w,int h,string label,int level,BuyFn buy)
        {
            GUI.Label(new Rect(x,y+h-4,w,22),label,new GUIStyle(small){alignment=TextAnchor.MiddleCenter,fontSize=13});
            bool maxed=level>=MaxUpgradeLevel;
            string priceLabel=maxed?"MÁXIMO":$"{UpgradeCost(level)} MONEDAS";
            if(GUI.Button(new Rect(x,y+h+18,w,30),priceLabel,new GUIStyle(button){fontSize=13})&&!maxed)buy();
        }
    }
}
