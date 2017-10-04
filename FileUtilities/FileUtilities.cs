using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;
using Varigence.DynamicObjects;
using Varigence.Biml.CoreLowerer.SchemaManagement;
using Varigence.Biml.Extensions;
using Varigence.Biml.Extensions.SchemaManagement;
using Varigence.Flow.FlowFramework;
using Varigence.Flow.FlowFramework.Utility;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Connection;
using Varigence.Languages.Biml.Cube;
using Varigence.Languages.Biml.Cube.Action;
using Varigence.Languages.Biml.Cube.Aggregation;
using Varigence.Languages.Biml.Cube.Calculation;
using Varigence.Languages.Biml.Cube.Partition;
using Varigence.Languages.Biml.Dimension;
using Varigence.Languages.Biml.Fact;
using Varigence.Languages.Biml.FileFormat;
using Varigence.Languages.Biml.LogProvider;
using Varigence.Languages.Biml.Measure;
using Varigence.Languages.Biml.MeasureGroup;
using Varigence.Languages.Biml.Metadata;
using Varigence.Languages.Biml.Project;
using Varigence.Languages.Biml.Platform;
using Varigence.Languages.Biml.Principal;
using Varigence.Languages.Biml.Script;
using Varigence.Languages.Biml.Table;
using Varigence.Languages.Biml.Task;
using Varigence.Languages.Biml.Transformation;
using Varigence.Languages.Biml.Transformation.Destination;

namespace ShannonLowder.Biml.FileUtilities
{
    
    public class File  {
        public char ColumnDelimiter { get; set; }
        public string FilePath { get; set; }
        public bool FirstRowHeader { get; set; }
        public int HeaderRowsToSkip { get; set; }
        public ImportResults ImportResults
        {
            get
            {
                return new ImportResults(this.TableNodes, this.SchemaNodes, this.SchemaErrors);
            }
        }
        public string Name { get; set; }
        public IList<ImportError> SchemaErrors { get; set; }
        public HashSet<AstSchemaNode> SchemaNodes { get; set; }
        public HashSet<AstTableNode> TableNodes { get; set; }
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
        public ImportResults GetFileSchema() {
            List<AstSchemaNode> schemaNodes = new List<AstSchemaNode>();
            List<AstTableNode> tableNodes = new List<AstTableNode>();

            AstSchemaNode astSchemaNode = new AstSchemaNode(null)
            {
                Name = "File",
                ForceDisableIncrementalChangeTracking = true
            };

            schemaNodes.Add(astSchemaNode);

            AstTableNode astTableNode = new AstTableNode(null)
            {
                Name = this.Name,
                Schema = astSchemaNode,
                ForceDisableIncrementalChangeTracking = true
            };
            tableNodes.Add(astTableNode);

            Interrogator i = new Interrogator();
            List<DestinationColumn> DestinationObject = i.ProcessFile(
                    this.FilePath, 
                    this.ColumnDelimiter,
                    this.FirstRowHeader,
                    this.HeaderRowsToSkip,
                    this.TextQualifier);

            foreach (var col in DestinationObject) {
                
                //    astTableNode.Columns.Add(tableColumn);
            }






            return new ImportResults(tableNodes, schemaNodes, this.SchemaErrors);
        }
               
    }
    
    
}
