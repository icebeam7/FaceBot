using Microsoft.Bot.Builder.FormFlow;
using System;

namespace FaceBot.Modelos
{
    [Serializable]
    public class Persona
    {
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string FotoURL { get; set; }
    }
}
