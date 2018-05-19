using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FaceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.DialogoFace());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            string messageType = message.GetActivityType();
            if (messageType == ActivityTypes.DeleteUserData)
            {
            }
            else if (messageType == ActivityTypes.ConversationUpdate)
            {
            }
            else if (messageType == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (messageType == ActivityTypes.Typing)
            {
            }
            else if (messageType == ActivityTypes.Ping)
            {
            }
            return null;
        }
    }
}