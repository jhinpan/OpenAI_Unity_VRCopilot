using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Act as a ai assistant in a content creation project and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";

        private void Start()
        {
            button.onClick.AddListener(SendReply);
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        public JObject ParseMessageToEntities(string message)
        {
            JObject output = new JObject();

            List<string> materials = new List<string> { "hardwood", "plywood", /*...*/ };
            List<string> styles = new List<string> { "minimalist", "minimalistic", /*...*/ };
            List<string> supercategories = new List<string> { "Cabinet_Shelf_Desk", "Table", /*...*/ };
    
            foreach (var material in materials)
            {
                if (Regex.IsMatch(message, @"\b" + material + @"\b", RegexOptions.IgnoreCase))
                {
                    output["material"] = material;
                    break;
                }
            }
    
            foreach (var style in styles)
            {
                if (Regex.IsMatch(message, @"\b" + style + @"\b", RegexOptions.IgnoreCase))
                {
                    output["style"] = style;
                    break;
                }
            }
    
            foreach (var supercategory in supercategories)
            {
                if (Regex.IsMatch(message, @"\b" + supercategory + @"\b", RegexOptions.IgnoreCase))
                {
                    output["supercategory"] = supercategory;
                    break;
                }
            }
    
            if (Regex.IsMatch(message, "main.generate_at_point", RegexOptions.IgnoreCase))
            {
                output["intent"] = "main.generate_at_point";
            }
    
            return output;
        }       
        
        
        private async void SendReply()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }
}
