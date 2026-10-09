using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuyuRun
{
    // Extracted from RunnerPrototype's existing wallet/revive rules. Keep save keys compatible.
    public static class RunnerProgress
    {
        public const int ReviveCost=5;
        public const string SierraScene="Level_Sierra_Integration";
        public const string SelvaScene="Level_Selva";
        public static bool EnterSierraMuseum;
        public static bool SierraUnlocked=>PlayerPrefs.GetInt("SuyuRun.Progress.Sierra",0)==1||PlayerPrefs.GetInt("SuyuRun.Progress.CostaComplete",0)==1||PlayerPrefs.GetInt("SuyuRun.Progress.SierraComplete",0)==1;
        public static bool SelvaUnlocked=>PlayerPrefs.GetInt("SuyuRun.Progress.Selva",0)==1||PlayerPrefs.GetInt("SuyuRun.Progress.SierraComplete",0)==1;
        public static bool CanRevive(int coins)=>coins+PlayerPrefs.GetInt("SuyuRun.Prototype.Bank",0)>=ReviveCost;
        public static bool TryRevive(ref int coins)
        {
            if(!CanRevive(coins))return false;
            int local=Mathf.Min(coins,ReviveCost),remaining=ReviveCost-local;coins-=local;
            if(remaining>0){PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",PlayerPrefs.GetInt("SuyuRun.Prototype.Bank",0)-remaining);PlayerPrefs.Save();}
            return true;
        }
        public static bool TrySpend(ref int coins,int cost)
        {
            if(cost<0||coins<cost)return false;
            coins-=cost;return true;
        }
        public static void Finish(string region,int coins,int score,bool diagnostic,out int bank,out int best)
        {
            bank=PlayerPrefs.GetInt("SuyuRun.Prototype.Bank")+coins;
            best=Mathf.Max(PlayerPrefs.GetInt("SuyuRun.Prototype.Best"),score);
            if(diagnostic)return;
            PlayerPrefs.SetInt("SuyuRun.Prototype.Bank",bank);PlayerPrefs.SetInt("SuyuRun.Prototype.Best",best);
            PlayerPrefs.SetInt("SuyuRun.Progress."+region+"Complete",1);
            PlayerPrefs.SetInt("SuyuRun.Progress."+region,1);
            if(region=="Costa")PlayerPrefs.SetInt("SuyuRun.Progress.Sierra",1);
            if(region=="Sierra")
            {
                PlayerPrefs.SetInt("SuyuRun.Progress.Selva",1);
                PlayerPrefs.SetInt("SuyuRun.Reward.ProtectiveSuit",1);
            }
            PlayerPrefs.Save();
        }
        public static bool OpenSierra()
        {
            if(!Application.CanStreamedLevelBeLoaded(SierraScene))
            {Debug.LogError("Sierra no está registrada en la lista de escenas. Preparar escena Sierra en Unity.");return false;}
            SceneManager.LoadScene(SierraScene);return true;
        }
        public static bool OpenSelva()
        {
            if(!Application.CanStreamedLevelBeLoaded(SelvaScene)){Debug.LogError("Preparar escena Selva en Unity.");return false;}
            SceneManager.LoadScene(SelvaScene);return true;
        }
    }
}
