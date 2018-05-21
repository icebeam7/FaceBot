using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using FaceBot.Servicios;
using FaceBot.Modelos;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.IO;

namespace FaceBot.Dialogs
{
    [LuisModel("modelID", "subscriptionKey", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class DialogoFace : LuisDialog<object>
    {
        public DialogoFace()
        {

        }

        public DialogoFace(ILuisService service): base(service)
        {
        }

        Persona Usuario;

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Hola, soy FaceBot. Intenta con otro mensaje, por ejemplo dime cómo te llamas y tu edad. También puedes decir 'Quiero registrar una foto' o 'Analiza imagen' para detectar la emoción de una persona en una foto.\n\n Intención detectada: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Saludar")]
        public Task Saludar(IDialogContext context, LuisResult result)
        {
            EntityRecommendation EntidadNombre, EntidadEdad;

            if (result.TryFindEntity("Nombre", out EntidadNombre))
            {
                Usuario = new Persona() { Nombre = EntidadNombre.Entity };

                if (result.TryFindEntity("builtin.age", out EntidadEdad))
                {
                    var edad = 0;
                    var cadena = EntidadEdad.Entity;
                    var datos = cadena.Split(' ');

                    int.TryParse(datos[0], out edad);
                    Usuario.Edad = edad;
                    SolicitarEdad(context, null);
                }
                else
                    PromptDialog.Text(context, SolicitarEdad, "¿Cuál es tu edad?");
            }
            else
                PromptDialog.Text(context, SolicitarNombre, "¿Cómo te llamas?");

            return Task.CompletedTask;
        }

        private async Task SolicitarNombre(IDialogContext context, IAwaitable<string> result)
        {
            var nombre = await result;

            var entidadNombre = new EntityRecommendation(type: "Nombre")
            {
                Entity = (nombre != null) ? nombre : "Juan Perez"
            };

            Usuario = new Persona() { Nombre = nombre };
            PromptDialog.Text(context, SolicitarEdad, "¿Cuál es tu edad?");
        }

        private async Task SolicitarEdad(IDialogContext context, IAwaitable<string> result)
        {
            if (Usuario.Edad == 0)
            {
                int edad = 0;
                int.TryParse(await result, out edad);
                Usuario.Edad = edad;
            }

            await context.PostAsync($"Bienvenido **{this.Usuario.Nombre}**. Eres más joven que yo, pues tienes **{this.Usuario.Edad}** años.");
        }

        [LuisIntent("EnviarImagen")]
        public async Task EnviarImagen(IDialogContext context, LuisResult result)
        {
            PromptDialog.Attachment(context, SolicitarImagen, "Muy bien. Envía la imagen");
        }

        private async Task SolicitarImagen(IDialogContext context, IAwaitable<IEnumerable<Attachment>> result)
        {
            var imagen = await result;

            if (imagen.Count() > 0)
            {
                Usuario.FotoURL = imagen.First().ContentUrl;
                await context.PostAsync($"Imagen recibida.");
            }
            else
                await context.PostAsync("Error. Fotografía no detectada");
        }

        private static async Task<Stream> GetImageStream(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var uri = new Uri(url);
                return await httpClient.GetStreamAsync(uri);
            }
        }

        [LuisIntent("AnalizarImagen")]
        public async Task AnalizarImagen(IDialogContext context, LuisResult result)
        {
            var stream = await GetImageStream(Usuario.FotoURL);
            var emocion = await ServicioFace.ObtenerEmocion(stream);

            await context.PostAsync($"**{Usuario.Nombre}** tu emoción es **{emocion.Nombre}** (Score: {emocion.Score})");
        }
    }
}
