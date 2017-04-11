using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AssFinetApi.Entrys;

namespace AssFinetApi {

    public class XpathSelector {
        public Encoding Encoding = new UTF8Encoding();

        readonly XElement _inputDocument;
        readonly XElement _configurationDocument;

        public XpathSelector( XElement inputDocument, XElement configurationDocument = null ) {
            _inputDocument = inputDocument;
            _configurationDocument = configurationDocument;
        }

        public DataTable SelectData( byte[] dataBytes = null ) {
            XmlSelection selectionFromConfituration = getXmlSelection();
            DataTable table = getDataTableGeneratedFromXPath( selectionFromConfituration );
            return table;
        }

        XmlSelection getXmlSelection() {
            string context = "";
            string key = "";
            List<XmlEntry> entryFromToList = new List<XmlEntry>();

            if( _configurationDocument != null )
            {
                context = trimLastBracket( _configurationDocument.Attributes()
                                                                 .Where( x => x.Name == XmlSelection.ContextString )
                                                                 .Select( x => x.Value )
                                                                 .First() );

                key = trimLastBracket( _configurationDocument.Attributes()
                                                                 .Where( x => x.Name == XmlSelection.Key )
                                                                 .Select( x => x.Value )
                                                                 .First() );
                entryFromToList = (
                    from element in _configurationDocument.Elements( XmlSelection.EntryString )
                    let xFromAttribute = element.Attribute( XmlSelection.FromString )
                    let xToAttribute = element.Attribute( XmlSelection.ToAttributeString )
                    where xFromAttribute != null && element != null && !string.IsNullOrEmpty( xFromAttribute.Value )
                    select new XmlEntry( trimLastBracket( xFromAttribute.Value ), trimLastBracket( xToAttribute?.Value ) ) )
                    .ToList();
            }

            return new XmlSelection( context, entryFromToList, key );
        }

        DataTable getDataTableGeneratedFromXPath( XmlSelection xmlSelection ) {
            XElement rootElement = new XElement( "Data" );
            List<string> keys = _inputDocument.XPathSelectElements( xmlSelection.selectionKey ).Select( x => x.Value ).ToList();

            foreach( XElement productElement in _inputDocument.XPathSelectElements( xmlSelection.Context ) ) {      
                if( !keys.Contains( productElement.Value ) ) continue;
                XElement selectionElement = new XElement( productElement.Name );
                foreach( var entry in xmlSelection.ColumnFromToNames )
                    createStructuredXmlGroupedByEntrys( productElement, entry, selectionElement );
                rootElement.Add( selectionElement );
            }

            return createDataTableFromMemoryStream( rootElement );
        }

        void createStructuredXmlGroupedByEntrys( XElement productElement, XmlEntry entry, XElement selectionElement ) {
            try {
                var xElementNode = XPathSelectElements( productElement, entry.@from );

                foreach( var nameObject in xElementNode ) {
                    XElement entryElement = new XElement( SetProperName( nameObject, entry.to ) );
                    entryElement.Add( nameObject.Value );
                    if( selectionWithNoDuplicateEntry( selectionElement, entryElement.Name.ToString() ) )
                        selectionElement.Add( entryElement );
                }
            } catch( Exception ex ) {
                Console.WriteLine( "Error: " + ex );
            }
        }

        IEnumerable<XElement> XPathSelectElements( XElement productElement, string entryFrom ) {
            IEnumerable xpathElements = (IEnumerable)productElement.XPathEvaluate( entryFrom );
            List<XElement> list = new List<XElement>();

            if( xpathElements.OfType<XAttribute>().Any() )
                foreach( XAttribute attribute in xpathElements.OfType<XAttribute>() )
                    addGeneratedXElementsToList( attribute.Name.ToString(), attribute.Value, list );
            else
                foreach( XElement element in xpathElements.OfType<XElement>() )
                    addGeneratedXElementsToList( element.Name.ToString(), element.Value, list );

            return list;
        }

        static void addGeneratedXElementsToList( string elementName, string elementValue, List<XElement> list ) {
            XElement xElement = new XElement( elementName );
            xElement.Add( elementValue );
            list.Add( xElement );
        }

        static bool selectionWithNoDuplicateEntry( XElement selectionElement, string entryElementName ) {
            return selectionElement.Element( entryElementName ) == null;
        }

        DataTable createDataTableFromMemoryStream( XElement productElements ) {
            var dataSet = new DataSet();
            using( var sw = new StringWriter() ) {
                using( var xw = new XmlTextWriter( sw ) ) {
                    productElements.WriteTo( xw );
                    using( var stream = new MemoryStream( Encoding.GetBytes( sw.ToString() ) ) )
                        dataSet.ReadXml( stream );
                }
            }

            return dataSet?.Tables[0];
        }

        static string SetProperName( XElement nameObject, string toName ) {
            if( string.IsNullOrEmpty( toName ) )
                return nameObject.Name.ToString();
            try {
                new XElement( toName );
            } catch( Exception ) {
                return nameObject.Name.ToString();
            }
            return toName;
        }

        static string trimLastBracket( string str ) {
            if( string.IsNullOrEmpty( str ) )
                return str;
            return str[str.Length - 1] == '/' ? str.TrimEnd( str[str.Length - 1] ) : str;
        }
    }

}