using CloudNimble.EasyAF.CodeGen.Generators.Base;
using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// Generates SimpleMessageBus message classes for entity CRUD operations.
    /// </summary>
    public class SimpleMessageBusGenerator : EntityGeneratorBase
    {

        #region Fields

        private readonly string _messageType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMessageBusGenerator"/> class.
        /// </summary>
        /// <param name="extraUsings">Additional using statements to include.</param>
        /// <param name="entityNamespace">The namespace where the generated messages will be placed.</param>
        /// <param name="entity">The entity composition metadata.</param>
        /// <param name="messageType">The type of message to generate (Base, Created, Updated, Deleted).</param>
        public SimpleMessageBusGenerator(List<string> extraUsings, string entityNamespace, EntityComposition entity, 
                                       string messageType)
            : base(extraUsings, entityNamespace, entity)
        {
            _messageType = messageType;
            
            // Use the entityNamespace directly as the target namespace (no suffix)
            Namespace = entityNamespace;
            
            AddUsings();
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the message class based on the configured message type.
        /// </summary>
        public override void Generate()
        {
            if (IsGenerated) return;
            
            Header();
            WriteUsings();
            NamespaceBegin(Namespace);
            
            switch (_messageType.ToLowerInvariant())
            {
                case "base":
                    GenerateDbEntityMessageBase();
                    break;
                case "created":
                    GenerateCreatedMessage();
                    break;
                case "updated":
                    GenerateUpdatedMessage();
                    break;
                case "deleted":
                    GenerateDeletedMessage();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown message type: {_messageType}");
            }
            
            NamespaceEnd();
            IsGenerated = true;
        }

        /// <summary>
        /// Writes the generated file to the specified directory.
        /// </summary>
        /// <param name="directory">The directory to write the file to.</param>
        /// <returns>The path of the written file.</returns>
        public string WriteFile(string directory = null)
        {
            var fileName = _messageType.ToLowerInvariant() == "base" 
                ? "DbEntityMessageBase" 
                : $"{Entity.EntityType.Name}{_messageType}";
                
            return WriteFile(fileName, directory);
        }

        #endregion

        #region Private Methods

        private void GenerateDbEntityMessageBase()
        {
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Base class for entity-based messages in the SimpleMessageBus system.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <typeparam name=\"T\">The type of entity contained in the message.</typeparam>");
            _writer.WriteLine("public abstract class DbEntityMessageBase<T> : MessageBase where T : class");
            _writer.WriteLine("{");
            _writer.WriteLine();
            _writer.Indent++;
            
            // Properties
            RegionBegin("Properties");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Gets or sets the entity associated with this message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("public T Entity { get; set; }");
            _writer.WriteLine();
            
            RegionEnd();
            
            // Constructors
            RegionBegin("Constructors");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Initializes a new instance of the <see cref=\"DbEntityMessageBase{T}\"/> class.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("protected DbEntityMessageBase() : base()");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Initializes a new instance of the <see cref=\"DbEntityMessageBase{T}\"/> class with a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine("protected DbEntityMessageBase(IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Initializes a new instance of the <see cref=\"DbEntityMessageBase{T}\"/> class with metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine("protected DbEntityMessageBase(string triggeredById, string correlationSource) : base()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("if (!string.IsNullOrWhiteSpace(triggeredById))");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Metadata[\"User.Id\"] = triggeredById;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            _writer.WriteLine("if (!string.IsNullOrWhiteSpace(correlationSource))");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Metadata[\"Correlation.Source\"] = correlationSource;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Initializes a new instance of the <see cref=\"DbEntityMessageBase{T}\"/> class with a parent message and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine("protected DbEntityMessageBase(IMessage parent, string triggeredById, string correlationSource) : base(parent)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("if (!string.IsNullOrWhiteSpace(triggeredById))");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Metadata[\"User.Id\"] = triggeredById;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            _writer.WriteLine("if (!string.IsNullOrWhiteSpace(correlationSource))");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Metadata[\"Correlation.Source\"] = correlationSource;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            RegionEnd();
            
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        private void GenerateCreatedMessage()
        {
            var entityName = Entity.EntityType.Name;
            var className = $"{entityName}Created";
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Message published when a new <see cref=\"{entityName}\"/> entity is created.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public class {className} : DbEntityMessageBase<{entityName}>");
            _writer.WriteLine("{");
            _writer.WriteLine();
            _writer.Indent++;
            
            // Constructors
            RegionBegin("Constructors");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public {className}() : base()");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}(IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the created entity.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was created.</param>");
            _writer.WriteLine($"public {className}({entityName} entity) : this()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the created entity and a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was created.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the created entity and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was created.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, string triggeredById, string correlationSource) : base(triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the created entity, parent message, and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was created.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, IMessage parent, string triggeredById, string correlationSource) : base(parent, triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            RegionEnd();
            
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        private void GenerateUpdatedMessage()
        {
            var entityName = Entity.EntityType.Name;
            var className = $"{entityName}Updated";
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Message published when a <see cref=\"{entityName}\"/> entity is updated.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public class {className} : DbEntityMessageBase<{entityName}>");
            _writer.WriteLine("{");
            _writer.WriteLine();
            _writer.Indent++;
            
            // Properties
            RegionBegin("Properties");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Gets or sets the dictionary of updated property values.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("public Dictionary<string, object> UpdatedValues { get; set; }");
            _writer.WriteLine();
            
            RegionEnd();
            
            // Constructors
            RegionBegin("Constructors");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public {className}() : base()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("UpdatedValues = new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}(IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("UpdatedValues = new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the updated entity and changed values.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was updated.</param>");
            _writer.WriteLine("/// <param name=\"updatedValues\">The dictionary of property values that were changed.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, Dictionary<string, object> updatedValues) : this()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.WriteLine("UpdatedValues = updatedValues ?? new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the updated entity, changed values, and parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was updated.</param>");
            _writer.WriteLine("/// <param name=\"updatedValues\">The dictionary of property values that were changed.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, Dictionary<string, object> updatedValues, IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.WriteLine("UpdatedValues = updatedValues ?? new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the updated entity, changed values, and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was updated.</param>");
            _writer.WriteLine("/// <param name=\"updatedValues\">The dictionary of property values that were changed.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, Dictionary<string, object> updatedValues, string triggeredById, string correlationSource) : base(triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.WriteLine("UpdatedValues = updatedValues ?? new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the updated entity, changed values, parent message, and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was updated.</param>");
            _writer.WriteLine("/// <param name=\"updatedValues\">The dictionary of property values that were changed.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, Dictionary<string, object> updatedValues, IMessage parent, string triggeredById, string correlationSource) : base(parent, triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.WriteLine("UpdatedValues = updatedValues ?? new Dictionary<string, object>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            RegionEnd();
            
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        private void GenerateDeletedMessage()
        {
            var entityName = Entity.EntityType.Name;
            var className = $"{entityName}Deleted";
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Message published when a <see cref=\"{entityName}\"/> entity is deleted.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public class {className} : DbEntityMessageBase<{entityName}>");
            _writer.WriteLine("{");
            _writer.WriteLine();
            _writer.Indent++;
            
            // Constructors
            RegionBegin("Constructors");
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public {className}() : base()");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}(IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the deleted entity.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was deleted.</param>");
            _writer.WriteLine($"public {className}({entityName} entity) : this()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the deleted entity and a parent message.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was deleted.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, IMessage parent) : base(parent)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the deleted entity and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was deleted.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, string triggeredById, string correlationSource) : base(triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Initializes a new instance of the <see cref=\"{className}\"/> class with the deleted entity, parent message, and metadata.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{entityName}\"/> entity that was deleted.</param>");
            _writer.WriteLine("/// <param name=\"parent\">The parent message for correlation.</param>");
            _writer.WriteLine("/// <param name=\"triggeredById\">The ID of the user who triggered this message.</param>");
            _writer.WriteLine("/// <param name=\"correlationSource\">The source system or service that generated this message.</param>");
            _writer.WriteLine($"public {className}({entityName} entity, IMessage parent, string triggeredById, string correlationSource) : base(parent, triggeredById, correlationSource)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Entity = entity;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            
            RegionEnd();
            
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        private void AddUsings()
        {
            ExtraUsings.Add("System");
            ExtraUsings.Add("System.Collections.Generic");
            ExtraUsings.Add("System.Collections.Concurrent");
            ExtraUsings.Add("CloudNimble.SimpleMessageBus.Core");
            
            // Don't add EDMX namespace - entities are always in Core namespace in EasyAF architecture
            // The Core namespace is passed in via extraUsings from the caller
        }

        #endregion

    }

}