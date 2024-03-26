using Microsoft.JSInterop;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Web.Ui.SpeechToText
{
    public class SpeechToTextService : BasicAutoStartable, ISpeechToTextService
    {
        public string SpokenLanguage { get; set; } = "zh-HK";

        // Dependencies
        readonly IJSRuntime _js;
        readonly IPandaMessenger _messenger;

        /// <summary>
        /// Creates a new instance of <see cref="SpeechToTextService" />
        /// </summary>
        public SpeechToTextService(IJSRuntime js, IPandaMessenger messenger)
        {
            _js = js;
            _messenger = messenger;
            _ = js.InvokeAsync<IJSObjectReference>(
                "import", "./Pages/Voice.razor.js");
        }
    }
}
