using System.Collections.Generic;

namespace BGLogPlugin
{
    class CardTag
    {
        public string entityName { get; set; }
        public string id { get; set; }
        public string zone { get; set; }
        public string zonePos { get; set; }
        public string cardId { get; set; }
        public string player { get; set; }
        public Dictionary<string, string> tags { get; set; }

        public CardTag()
        {
            tags = new Dictionary<string, string>();
        }
    }
}
