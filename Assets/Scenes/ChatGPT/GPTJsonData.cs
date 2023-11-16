using System;
using System.Collections.Generic;
using TMPro;

namespace GPTJsonData
{
    [System.Serializable]
    public class Param
    {
        public string furniture_type; //supercategory
        public string furniture_category;
        public string furniture_style;
        public string furniture_material;
        public string point_confirm_choice;
        public int duplicate_number = 1;
    }
    

    [System.Serializable]
    public class ResponseMessage
    {
        public Text text;
    }

    [System.Serializable]
    public class Text
    {
        public string[] text;
    }

    [System.Serializable]
    public class Match
    {
        public Intent intent;
        public string @event;
        public string resolvedInput;
    }

    [System.Serializable]
    public class Intent
    {
        public string name;
        public string displayName;
    }

    [System.Serializable]
    public enum ResponseType
    {
        RESPONSE_TYPE_UNSPECIFIED,
        PARTIAL,
        FINAL
    }

    [System.Serializable]
    public class GPTOutput
    {
        public Intent intent;
        public Param parameters;
    }
}
