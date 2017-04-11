using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AssFinetApi {
    public static class XmlHelper {

        public static string GetConfigValue() {
            var configPath = System.Configuration.ConfigurationManager.AppSettings["xsdFilePath"];
            var fullConfigPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, configPath );
            return validateConfigFile( fullConfigPath );
        }

        static string validateConfigFile( string fullConfigPath ) {
            return File.Exists( fullConfigPath ) ? 
                fullConfigPath : 
                configFileNotFound();
        }

        static string configFileNotFound() {
            return "Config file not found";
        }

        public static void RemoveDuplicatesFromDataTable( ref DataTable table ) {

            Dictionary<string, string> uniquenessDict = new Dictionary<string, string>( table.Rows.Count );
            int rowIndex = 0;
            DataRow row;
            DataRowCollection rows = table.Rows;

            while( rowIndex < rows.Count ) {
                row = rows[rowIndex];
                var stringBuilder = new StringBuilder();
                stringBuilder.Append( (string)row[table.Columns[0].ColumnName] );

                if( uniquenessDict.ContainsKey( stringBuilder.ToString() ) ) {
                    rows.Remove( row );
                } else {
                    uniquenessDict.Add( stringBuilder.ToString(), string.Empty );
                    rowIndex++;
                }
            }
        }
    }

    public class FileHelper
    {
        static string _FilePath;
        static string _Xpath;

        public string FilePath {
            get { return _FilePath; }
            set { _FilePath = value;  }
        }

        public string Xpath {
            get { return _Xpath; }
            set { _Xpath = value; }
        }

        public static bool FilesExists()
        {
            return File.Exists( Path.Combine( _FilePath ) ) &&
                   File.Exists( Path.Combine( _Xpath ) );
        }
    }




}