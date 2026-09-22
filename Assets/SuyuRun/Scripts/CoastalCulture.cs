namespace SuyuRun
{
    public static class CoastalCulture
    {
        public sealed class Entry
        {
            public string id,name,place,teaser,description,source;
            public Entry(string id,string name,string place,string teaser,string description,string source)
            {this.id=id;this.name=name;this.place=place;this.teaser=teaser;this.description=description;this.source=source;}
        }
        public static readonly Entry[] Entries={
            new Entry("huaco_moche","Huaco retrato mochica","OBJETO · COSTA NORTE · CULTURA MOCHE",
                "Un rostro modelado en barro puede conservar la memoria de una sociedad.",
                "Los ceramistas mochicas representaron rostros humanos con gran detalle. Sus huacos retrato son característicos del arte de la costa norte del antiguo Perú. La ilustración del juego es una interpretación, no la reproducción de una pieza arqueológica concreta.",
                "https://museolarco.blogspot.com/2009/06/huaco-retrato-mochica.html"),
            new Entry("textil_paracas","Manto Paracas","OBJETO · COSTA SUR · PENÍNSULA DE PARACAS",
                "Los hilos también cuentan historias: descubre para qué servían estos mantos.",
                "En la costa sur, los mantos Paracas formaban parte de envolturas funerarias. Sus bordados combinan colores y figuras de significado simbólico. El ambiente seco del desierto favoreció la conservación de estos textiles. El diseño del juego no copia un motivo sagrado específico.",
                "https://www.museolarco.org/exposicion/exposicion-permanente/obras-maestras/manto-paracas/"),
            new Entry("nicomedes","Nicomedes Santa Cruz","PERSONAJE · LIMA · 1925–1992",
                "La poesía y la tradición oral también son patrimonio. ¿Quién ayudó a difundirlas?",
                "Nicomedes Santa Cruz fue un poeta, decimista, folclorista e intelectual afroperuano nacido en Lima. Su trabajo contribuyó a reivindicar la cultura afroperuana. Esta ficha conecta la música de Costa con quienes preservaron y difundieron su memoria cultural.",
                "https://memoriaperu.bnp.gob.pe/micrositio/nicomedes-santacruz"),
            new Entry("maria_reiche","María Reiche","PERSONAJE · NASCA, ICA · INVESTIGACIÓN Y CONSERVACIÓN",
                "Una investigadora dedicó décadas a cuidar los trazos del desierto costero.",
                "La científica y matemática María Reiche dedicó más de cincuenta años al estudio y la conservación de las Líneas de Nasca. Se incluye por su relación con el patrimonio de la costa sur, no por atribuirle el origen de los geoglifos ni presentarla como una figura de la cultura Nasca antigua.",
                "https://www.miraflores.gob.pe/municipalidad-de-miraflores-rindio-homenaje-a-maria-reiche-a-25-anos-de-su-fallecimiento/")
        };
    }
}
