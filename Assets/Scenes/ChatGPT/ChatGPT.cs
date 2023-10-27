using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
        List<JObject> savedEntities = new List<JObject>();
        private List<JObject> modelInfo;
        private string modelBasePath = "D:/3D-FRONT/3D-FUTURE-model/";

        private TextToSpeech textToSpeech;

        private List<ChatMessage> messages = new List<ChatMessage>();

        private string prompt = 
        "Act as a design assistant in a content creation project. Respond to user queries by recognizing the following intents and mapping them to specific actions or outputs: " +
        "1. main.delete: Recognize phrases like 'move this chair' or 'I don't want this table anymore' to delete specific furniture items. " +
        "2. main.batch_delete: Phrases like 'I want everything gone except this dresser' should trigger a batch deletion while keeping a specified item. " +
        "3. main.duplicate: If the user says 'Duplicate this item two times', produce additional copies of a given item. " +
        "4. main.regenerate: Understand phrases such as 'Let's start over, I don't like this.' to regenerate the design. " +
        "5. response.confirm: Recognize user preferences with phrases like 'How about the right one?' " +
        "6. main.generate_at_point: Generate specific items when prompted by phrases such as 'Begin with generating a rustic bed of pine here'. " +
        "For intents that are undetected or conflict, provide an appropriate response. " +
        "We're embarking on a complex and innovative furniture design project. Our aim is to provide a comprehensive experience. " +
        "Entities and their Possible Values: " +
        "1. Materials: hardwood, plywood, timber, wood, wooden, aluminum, iron, metal, metallic, steel, acrylic, plastic, polyethylene, PVC, cloth, fabric, textile, upholstery, hide, leather, nubuck, skin, clear, crystal, glass, transparent, granite, marble, quartz, stone, bamboo, cane, rattan, wicker, elastic, latex, neoprene, rubber. " +
        "2. Styles: minimalist, minimalistic, Ming Qing, Modern, Japanese, Southeast Asia, vintage, retro, Chinoiserie, Industrial, Mediterranean, New Chinese. " +
        "3. Supercategory: Cabinet_Shelf_Desk, Table, Sofa, Chair, Bed, Lighting, Pier_Stool, Stool. " +
        "4. Category: Children Cabinet, Nightstand, Bookcase, Jewelry Armoire, Wardrobe, Coffee Table, Corner/Side Table, Sideboard, Side Cabinet, Console Table, Wine Cabinet, TV Stand, Drawer Chest, Corner cabinet. " +
        "For intents like 'main.generate_at_point' and 'main.regenerate', produce a specific JSON output, e.g., {'supercategory': 'sofa', 'style': 'minimalist'}. " +
        "Unleash your creativity and surprise us!";


        private void Start()
        {
            button.onClick.AddListener(() => { Debug.Log("Button clicked."); SendReply(); });
            Whisper.OnMessageReady += ReceiveWhisperMessage;
            
            // // Load model_info_VR.json
            // string jsonPath = "Assets/Resources/model_info_VR.json";
            // string json = File.ReadAllText(jsonPath);
            // JObject jsonObj = JObject.Parse(json);
            // modelInfo = jsonObj["data"]?.ToObject<List<JObject>>().Take(100).ToList(); // Take only first 100

            textToSpeech = GetComponent<TextToSpeech>();
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
        
        private void ReceiveWhisperMessage(string message)
        {
            // Set the received message from Whisper to the inputField
            inputField.text = message;
    
            SendReply();
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
        

        private void SaveParsedEntities(JObject entities)
        {
            savedEntities.Add(entities);
            
            foreach (JObject entity in savedEntities)
            {
                Debug.Log(entity.ToString());
            }
            
            // foreach (var model in modelInfo)
            // {
            //     bool isMatch = true;
            //     foreach (var entity in entities)
            //     {
            //         if (model[entity.Key] == null || model[entity.Key].ToString() != entity.Value.ToString())
            //         {
            //             isMatch = false;
            //             break;
            //         }
            //     }
            //     if (isMatch)
            //     {
            //         string modelID = model["model_id"].ToString();
            //         string modelPath = Path.Combine(modelBasePath, modelID, "raw_model.obj");
            //         Mesh loadedMesh = ObjLoader.Load(modelPath);
            //         GameObject loadedModel = new GameObject();
            //         loadedModel.AddComponent<MeshFilter>().mesh = loadedMesh;
            //         loadedModel.AddComponent<MeshRenderer>();
            //         
            //         // Set the spawning position for the loadedModel
            //         loadedModel.transform.position = new Vector3(0, 0, 0);
            //     }
            //     else
            //     {
            //         
            //     }
            // }
        }



        private async void SendReply()
        {
            Debug.Log("Entered SendReply");
            
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

            // Before API call
            Debug.Log("About to make API call");
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0301",
                Messages = messages
            });

            // After API call
            Debug.Log("API call completed");
            
            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);
                AppendMessage(message);
                Debug.Log("Received Choices from API");
                JObject parsedEntities = ParseMessageToEntities(message.Content);

                textToSpeech.SpeakText(message.Content);
                
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
        
        
        private void OnDestroy()
        {
            Whisper.OnMessageReady -= ReceiveWhisperMessage;
        }

    }
}