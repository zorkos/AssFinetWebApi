using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssFinetApi.Entrys {
    public class XmlSelection {

        public static readonly string ContextString = "context";
        public static readonly string EntryString = "Entry";
        public static readonly string FromString = "from";
        public static readonly string ToAttributeString = "to";
        public static readonly string Key = "key";

        public string Context { get; set; }
         public string selectionKey { get; set; }
        public List<XmlEntry> ColumnFromToNames { get; set; }

        public XmlSelection( string context, List<XmlEntry> columnFromToNames, string key = null ) {
            Context = context;
            ColumnFromToNames = columnFromToNames;
            selectionKey = key;
        }

    }
}
