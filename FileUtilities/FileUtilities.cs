using ShannonLowder.Biml;
using System.Collections.Generic;
using System.IO;
using Varigence.Languages.Biml.Table;

namespace ShannonLowder.Biml.FileUtilities
{
    
    public class File  {
        public char ColumnDelimiter { get; set; }
        public string FilePath { get; set; }
        public bool FirstRowHeader { get; set; }
        public int HeaderRowsToSkip { get; set; }

        public string Name { get; set; }

        public AstTableNode TableNode { get; set; }
        public string TextQualifier { get; set; }
        
        //minimally you need a file path and delimiter
        public File (string filePath, char columnDelimiter) {
            ColumnDelimiter = columnDelimiter;
            FilePath = filePath;
            FirstRowHeader = false;
            HeaderRowsToSkip = 0;
            Name = Path.GetFileNameWithoutExtension(filePath);
            TextQualifier = null;
        } 
        public AstTableNode GetFileSchema() {
            //default the schema to "File"
            AstSchemaNode astSchemaNode = new AstSchemaNode(null)
            {
                Name = "File",
                ForceDisableIncrementalChangeTracking = true
            };
            
            AstTableNode astTableNode = new AstTableNode(null)
            {
                Name = this.Name,
                Schema = astSchemaNode,
                ForceDisableIncrementalChangeTracking = true
            };
            this.TableNode = astTableNode;



            Interrogator i = new Interrogator();
            List<DestinationColumn> DestinationObject = i.ProcessFile(
                    this.FilePath, 
                    this.ColumnDelimiter,
                    this.FirstRowHeader,
                    this.HeaderRowsToSkip,
                    this.TextQualifier);

            foreach (DestinationColumn col in DestinationObject) {
                AstTableColumnNode currentColumn = new AstTableColumnNode(astTableNode)
                {
                    ForceDisableIncrementalChangeTracking = true
                };
                //set up the column
                currentColumn.Name = col.Name;
                if(col.MaxLength != null)
                    currentColumn.Length = int.Parse(col.MaxLength.ToString());
                if(col.Precision != null)
                    currentColumn.Precision = int.Parse(col.Precision.ToString());
                //I need my data type converter
                ConversionUtility cu = new ConversionUtility()

                currentColumn.DataType =  
                    //System.Data.DbType
                    //cu.Convert(SourceSystem.SqlServer, SourceSystem.Biml, col.DataType);

                

                currentColumn.IsNullable = col.Nullable;
                //add the column to the table
                this.TableNode.Columns.Add(currentColumn);
            }






            return this.TableNode;
        }
               
    }
    
    
}
