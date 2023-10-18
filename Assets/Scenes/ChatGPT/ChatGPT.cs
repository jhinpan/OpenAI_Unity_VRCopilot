using UnityEngine;
using UnityEngine.UI;
using System.IO;
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

        private string prompt =
            "Act as a ai assistant in a content creation project and reply to the questions. Don't break character. Don't ever mention that you are an AI model." +
            "We're embarking on a complex and innovative furniture design project. Our aim is to encompass a multitude of entities and intents that can be customized to generate specific JSON outputs. " +
            "We're employing a variety of materials, styles, and furniture categories to provide a comprehensive experience." +
            "Entities and their Possible Values:" +
            "1. Materials: hardwood, plywood, timber, wood, wooden, aluminum, iron, metal, metallic, steel, acrylic, plastic, polyethylene, PVC, cloth, fabric, textile, upholstery, hide, leather, nubuck, skin, clear, crystal, glass, transparent, granite, marble, quartz, stone, bamboo, cane, rattan, wicker, elastic, latex, neoprene, rubber." +
            "2. Styles: minimalist, minimalistic, Ming Qing, Modern, Japanese, Southeast Asia, vintage, retro, Chinoiserie, Industrial, Mediterranean, New Chinese." +
            "3. Supecategory: Cabinet_Shelf_Desk, Table, Sofa, Chair, Bed, Lighting, Pier_Stool, Stool." +
            "4. Category: Children Cabinet, Nightstand, Bookcase, Jewelry Armoire, Wardrobe, Coffee Table, Corner/Side Table, Sideboard, Side Cabinet, Console Table, Wine Cabinet, TV Stand, Drawer Chest, Corner cabinet." +
            "The scope of intents for this project is unlimited; it's confined only by the mentioned entities. Feel free to imagine, synthesize, and create unique furniture pieces, settings, or arrangements based on these entities. The only requirement is that the resulting conversation should be able to be translated into a JSON object that incorporates the mentioned entities." +
            "For instance, a sentence like 'Let's create a minimalist sofa from scratch here,' should produce a JSON output like {“supercategory”: “sofa”, “style”: “minimalist”}. Go ahead, unleash your creativity and surprise us!";

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

            // Define all entity lists
            List<string> materials = new List<string>
            {
                "hardwood", "plywood", "timber", "wood", "wooden", "aluminum", "iron",
                "metal", "metallic", "steel", "acrylic", "plastic", "polyethylene",
                "PVC", "cloth", "fabric", "textile", "upholstery", "hide", "leather",
                "nubuck", "skin", "clear", "crystal", "glass", "transparent", "granite",
                "marble", "quartz", "stone", "bamboo", "cane", "rattan", "wicker",
                "elastic", "latex", "neoprene", "rubber"
            };

            List<string> styles = new List<string>
            {
                "minimalist", "minimalistic", "Ming Qing", "Modern", "Japanese",
                "Southeast Asia", "vintage", "retro", "Chinoiserie", "Industrial",
                "Mediterranean", "New Chinese"
            };

            List<string> supercategories = new List<string>
            {
                "Cabinet_Shelf_Desk", "Table", "Sofa", "Chair", "Bed", "Lighting",
                "Pier_Stool", "Stool"
            };

            List<string> categories = new List<string>
            {
                "Children Cabinet", "Nightstand", "Bookcase", "Jewelry Armoire",
                "Wardrobe", "Coffee Table", "Corner/Side Table", "Sideboard",
                "Side Cabinet", "Console Table", "Wine Cabinet", "TV Stand",
                "Drawer Chest", "Corner cabinet"
            };

            // Check each entity list for matches in the message
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

            foreach (var category in categories)
            {
                if (Regex.IsMatch(message, @"\b" + category + @"\b", RegexOptions.IgnoreCase))
                {
                    output["category"] = category;
                    break;
                }
            }

            return output;
        }

        List<JObject> savedEntities = new List<JObject>();

        private void SaveParsedEntities(JObject entities)
        {
            savedEntities.Add(entities);
            
            foreach (JObject entity in savedEntities)
            {
                Debug.Log(entity.ToString());
            }
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
                Model = "gpt-3.5-turbo-0301",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);
                AppendMessage(message);
                
                JObject parsedEntities = ParseMessageToEntities(message.Content);

                // Add this parsed data to a collection to be used later or save to file.
                SaveParsedEntities(parsedEntities);
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