using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AssFinetApi.Models;

namespace AssFinetApi.Controllers
{

    [RoutePrefix( "api/XmlValidator" )]
    public class XmlValidatorController : ApiController
    {

        //[Route( "" )]
        //public object Geti() {
        //    //return XmlHelper.GetConfigValue();
        //    var partners = new object[100];
        //    using( var conn = new SqlConnection( getConnectionString() ) ) {
        //        conn.Open();
        //        var cmd = new SqlCommand( "SELECT PartnerID, Name, Postleitzahl, Ort, PartnerID, PartnernummerVM, Strasse From Partner", conn );
        //        using( var oReader = cmd.ExecuteReader() )
        //        {
        //            int i = 0;
        //            while( oReader.Read() ) {
        //                object partner = new {
        //                    PartnerID = oReader["PartnerID"].ToString(),
        //                    Name = oReader["Name"].ToString(),
        //                    Postleitzahl = oReader["Postleitzahl"],
        //                    Ort = oReader["Ort"].ToString(),
        //                    PartnernummerVM = oReader["PartnernummerVM"].ToString(),
        //                    Strasse = oReader["Strasse"].ToString()
        //                };
        //                partners[i] = partner;
        //                i++;
        //            }
        //        }
        //    }
        //    return partners;
        //}

        // string strasse, string postleitzahl, string ort, string kommunikationsadresse
        [Route( "" )]
        public List<Partner> Get() {
            //return XmlHelper.GetConfigValue();
            var partners = new List<Partner>();
            using( var conn = new SqlConnection( getConnectionString() ) ) {
                conn.Open();
                var cmd = new SqlCommand( "SELECT PartnerID, Name, Postleitzahl, Ort, PartnerID, PartnernummerVM, Strasse From Partner", conn );
                using( var oReader = cmd.ExecuteReader() ) {
                    while( oReader.Read() )
                    {
                        Partner partner = new Partner( 
                            oReader["PartnerID"].ToString(), oReader["Name"].ToString(),
                            oReader["PartnernummerVM"].ToString(), oReader["Strasse"].ToString(), 
                            oReader["Postleitzahl"].ToString(), oReader["Ort"].ToString(), "" );
                            partners.Add( partner );
                    }
                }
            }
            return partners;
        }




        [Route( "{id:int}" )]
        public object Get( int id )
        {
            return new { name = "" + id, link = "a"};
        }

        [Route( "" )]
        public HttpResponseMessage Post( [FromBody]FileHelper fileInput ) {
            XmlDocument document = null;
            if(fileInput == null)
                return respondBadRequest();
            if( !FileHelper.FilesExists()) 
                return respondBadRequest();
            if( isValidatedFileInCorrect( fileInput, ref document ))
                 return respondBadRequest();

            var dataTable = loadDataTableFromSelection( fileInput, document );
            insertDataTableIntoDb( dataTable );

            return respondWithOkReqest();
        }

        #region ValidateXml
        bool isValidatedFileInCorrect( FileHelper fileInput, ref XmlDocument document ) {
            bool xmlDocumentHasErrors = false;
            XmlReaderSettings settings;
            getXsdSchema( out settings );
            document = generateXmlDocument( fileInput );

            document.Validate( ( sender, args ) => {
                //Console.WriteLine( $"{args.Message}" );
                xmlDocumentHasErrors = true;
            } );

            return xmlDocumentHasErrors;
        }

        static XmlDocument generateXmlDocument( FileHelper fileInput ) {
            XmlDocument document = new XmlDocument();
            document.Load( XmlReader.Create( fileInput.FilePath ) );
            document.Schemas = new XmlSchemaSet( document.NameTable );
            document.Schemas.Add( "http://www.bipro.net/namespace", XmlHelper.GetConfigValue() );
            return document;
        }

        void getXsdSchema( out XmlReaderSettings settings ) {
            settings = new XmlReaderSettings();
            settings.Schemas.Add( "http://www.bipro.net/namespace", XmlHelper.GetConfigValue() );
            settings.ValidationType = ValidationType.Schema;
        }

        #endregion

        #region Request Methods
        static HttpResponseMessage respondBadRequest() {
            return new HttpResponseMessage( HttpStatusCode.BadRequest );
        }

        static HttpResponseMessage respondWithOkReqest() {
            return new HttpResponseMessage( HttpStatusCode.OK );
        }

        #endregion

        #region DataTable Methods

        DataTable loadDataTableFromSelection( FileHelper fileInput, XmlDocument document) {            
            XpathSelector selection = new XpathSelector( XElement.Parse( document.InnerXml ), XElement.Load( fileInput.Xpath ) );
            DataTable result = selection.SelectData();
            result = result.Rows.Cast<DataRow>()
                .Where( row => !row.ItemArray.All( field => field is DBNull || string.IsNullOrWhiteSpace( field as string ) ) )
                .CopyToDataTable();
            XmlHelper.RemoveDuplicatesFromDataTable( ref result );

            return result;
        }

        void insertDataTableIntoDb( DataTable dataTable ) {
            foreach( DataRow row in dataTable.Rows ) {
                //TODO CHANGE NEEDED
                Partner partner = new Partner( row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString(), row[5].ToString(), "2" );
                using( var conn = new SqlConnection( getConnectionString() ) ) {
                    conn.Open();
                    var cmd = generateSqlCommand( conn, partner);   
                    cmd.ExecuteNonQuery();
                }
            }
        }
 
        static SqlCommand generateSqlCommand( SqlConnection conn, Partner partner ) {
            var cmd = new SqlCommand( countDbRows(), conn );
            cmd = new SqlCommand( getExistingRows( partner, cmd ) == 0 ? getInsertStatment() : getUpdateStatement(), conn );       
            cmd.Parameters.AddWithValue( "@PartnerID", partner.PartnerID );
            cmd.Parameters.AddWithValue( "@Name", partner.Name );
            cmd.Parameters.AddWithValue( "@PartnernummerVM", partner.PartnernummerVM );
            cmd.Parameters.AddWithValue( "@Ort", partner.Ort );
            cmd.Parameters.AddWithValue( "@Strasse", partner.Strasse );
            cmd.Parameters.AddWithValue( "@Postleitzahl", partner.Postleitzahl );
            cmd.Parameters.AddWithValue( "@Kommunikationsadresse", partner.Kommunikationsadresse );
            return cmd;
        }

        static int getExistingRows( Partner partner, SqlCommand cmd ) {
            cmd.Parameters.AddWithValue( "@PartnerID", partner.PartnerID );
            var reader = cmd.ExecuteReader();
            int counter = reader.FieldCount;
            reader.Close();
            return counter;
        }

        static string getConnectionString() {
            return @"Data Source=AALPERFLT002\SQLOCALSERVER;Initial Catalog=AssFinetDemo;Integrated Security=True";
        }

        static string countDbRows(){
            return "SELECT COUNT(PartnerID) FROM Partner Where PartnerID='@PartnerID';";
        }

        static string getInsertStatment(){
            return "INSERT INTO Partner (PartnerID, Name, PartnernummerVM, Ort, Strasse, Postleitzahl, Kommunikationsadresse) " +
                   "VALUES (@PartnerID, @Name, @PartnernummerVM, @Ort, @Strasse, @Postleitzahl, @Kommunikationsadresse)";
        }

        static string getUpdateStatement() {
            return "UPDATE Partner SET Name=@Name, PartnernummerVM=@PartnernummerVM, Ort=@Ort, " +
                   "Strasse=@Strasse, @Postleitzahl=Postleitzahl, Kommunikationsadresse=@Kommunikationsadresse " +
                   "WHERE PartnerID = @PartnerID";
        }

        #endregion
    }

     
}
