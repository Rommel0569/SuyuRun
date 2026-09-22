using UnityEngine;
namespace SuyuRun
{
    [CreateAssetMenu(menuName="SuyuRun/Selected music library")]
    public sealed class SelectedMusic:ScriptableObject
    {
        public AudioClip costa,sierra,selva,inicio;
        [TextArea] public string provenance="Archivos proporcionados por el usuario. Documentar créditos y permisos antes de distribuir.";
    }
}
