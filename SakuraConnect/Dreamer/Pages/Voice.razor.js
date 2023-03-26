let recognizing = false;

const SpeechRecognition=window.webkitSpeechRecognition;    
let recognition = new SpeechRecognition();
recognition.continuous = true;
recognition.interimResults = true;
//------------------
recognition.onstart=function(){
    console.log("it work");
};

recognition.onresult=function(event){
    const last = event.results.length - 1;
    const res = event.results[last];
    const text = res[0].transcript;
    if (res.isFinal) {
        console.log("processing ....");
        console.log("You said:"+ text);
    } 
    else
    {
        console.log("listening:"+text);
    }
    DotNet.invokeMethodAsync('SakuraConnect.Dreamer', 'Recognize_OnResult', text)
            .then(data => {
            console.log(data);
            });
};

recognition.onend = function() {
    console.log("voice recognition terminated");
    if (recognizing) {
        recognition.start();
    }
};

window.startRecognition = (lang) => {
    recognition.lang = lang;
    recognizing = true;
    recognition.start();
}

window.stopRecognition = () => {
    recognizing = false;
    recognition.stop();
}