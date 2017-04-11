namespace AssFinetApi.Entrys {
    public class XmlEntry {
        public string from { get; set; }
        public string to { get; set; }
        public string extraContent { get; set; }

        public XmlEntry( string from, string to = null, string extraContent = null ) {
            this.from = from;
            this.to = to;
            this.extraContent = extraContent;
        }

    }
}