namespace 亂七八糟.Prompt
{
    /// <summary>
    /// <seealso cref="https://learn.microsoft.com/en-us/dotnet/api/system.speech.synthesis.prompt.-ctor?view=net-9.0-pp"/>
    /// </summary>
    public class SpeechSynthesizer
    {
#if WINDOWS
        /// <summary>
        /// <seealso cref="https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speech-synthesis-markup-structure"/>
        /// </summary>
        /// <param name="text"></param>
        public static void Speak(string text)
        {
            using (System.Speech.Synthesis.SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {

                // Configure the audio output.   
                synth.SetOutputToDefaultAudioDevice();

                // Build an SSML prompt in a string.  
                string fileName = "<speak version=\"1.0\" ";
                fileName += "xmlns=\"http://www.w3.org/2001/10/synthesis\" ";
                fileName += "xml:lang=\"en-US\">";
                fileName += "Say a name for the new file <mark name=\"fileName\" />.";
                fileName += "</speak>";

                // Create a Prompt object from the string.  
                System.Speech.Synthesis.Prompt ssmlFile = new System.Speech.Synthesis.Prompt(fileName, System.Speech.Synthesis.SynthesisTextFormat.Ssml);

                // Speak the contents of the SSML prompt.  
                synth.Speak(ssmlFile);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
#endif
}
