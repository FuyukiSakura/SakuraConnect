const SpeechRecognition=window.webkitSpeechRecognition;    
var recognition = new SpeechRecognition();

window.startRecognition = (lang) => {
    recognition.lang = lang;
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
    recognition.start();
}

window.stopRecognition = () => {
    recognition.stop();
}