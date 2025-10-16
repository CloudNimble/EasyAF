// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel
{
    internal class FunctionImportElement : Function
    {
        private string _unresolvedEntitySet;
        private bool _entitySetPathDefined;
        private EntityContainer _container;
        private EntityContainerEntitySet _entitySet;
        private bool? _isSideEffecting;

        internal FunctionImportElement(EntityContainer container)
            : base(container.Schema)
        {
            if (Schema.DataModel
                == SchemaDataModelOption.EntityDataModel)
            {
                OtherContent.Add(Schema.SchemaSource);
            }

            _container = container;

            // By default function imports are non-composable.
            _isComposable = false;
        }

        public override bool IsFunctionImport
        {
            get { return true; }
        }

        public override string FQName
        {
            get { return _container.Name + "." + Name; }
        }

        public override string Identity
        {
            get { return base.Name; }
        }

        public EntityContainer Container
        {
            get { return _container; }
        }

        public EntityContainerEntitySet EntitySet
        {
            get { return _entitySet; }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.EntitySet))
            {
                if (Utils.GetString(Schema, reader, out var entitySetName))
                {
                    _unresolvedEntitySet = entitySetName;
                }
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.EntitySetPath))
            {
                if (Utils.GetString(Schema, reader, out var entitySetPath))
                {
                    // EF does not support this EDM 3.0 attribute, we only use it for validation.
                    _entitySetPathDefined = true;
                }
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.IsBindable))
            {
                // EF does not support this EDM 3.0 attribute, so ignore it.
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.IsSideEffecting))
            {
                // Even though EF does not support this attribute, we want to remember the value in order to throw an error
                // in case user specifies IsComposable = true and IsSideEffecting = true.
                var isSideEffecting = true;
                if (HandleBoolAttribute(reader, ref isSideEffecting))
                {
                    _isSideEffecting = isSideEffecting;
                }
                return true;
            }

            return false;
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            ResolveEntitySet(this, _unresolvedEntitySet, ref _entitySet);
        }

        internal void ResolveEntitySet(SchemaElement owner, string unresolvedEntitySet, ref EntityContainerEntitySet entitySet)
        {
            Debug.Assert(IsFunctionImport, "Only FunctionImport elkements specify EntitySets");
            Debug.Assert(null != _container, "function imports must know container");

            // resolve entity set
            if (null == entitySet
                && null != unresolvedEntitySet)
            {
                entitySet = _container.FindEntitySet(unresolvedEntitySet);

                if (null == entitySet)
                {
                    owner.AddError(
                        ErrorCode.FunctionImportUnknownEntitySet,
                        EdmSchemaErrorSeverity.Error,
                        Strings.FunctionImportUnknownEntitySet(unresolvedEntitySet, FQName));
                }
            }
        }

        internal override void Validate()
        {
            base.Validate();

            ValidateFunctionImportReturnType(this, _type, CollectionKind, _entitySet, _entitySetPathDefined);

            if (_returnTypeList is not null)
            {
                foreach (var returnType in _returnTypeList)
                {
                    Debug.Assert(returnType.Type is not null, "FunctionImport/ReturnType element must not have subelements.");

                    ValidateFunctionImportReturnType(
                        returnType, returnType.Type, returnType.CollectionKind, returnType.EntitySet, returnType.EntitySetPathDefined);
                }
            }

            if (_isComposable
                && _isSideEffecting.HasValue
                && _isSideEffecting.Value)
            {
                AddError(
                    ErrorCode.FunctionImportComposableAndSideEffectingNotAllowed,
                    EdmSchemaErrorSeverity.Error,
                    Strings.FunctionImportComposableAndSideEffectingNotAllowed(FQName));
            }

            if (_parameters is not null)
            {
                foreach (var p in _parameters)
                {
                    if (p.IsRefType
                        || p.CollectionKind != CollectionKind.None)
                    {
                        AddError(
                            ErrorCode.FunctionImportCollectionAndRefParametersNotAllowed,
                            EdmSchemaErrorSeverity.Error,
                            Strings.FunctionImportCollectionAndRefParametersNotAllowed(FQName));
                    }

                    if (!p.TypeUsageBuilder.Nullable)
                    {
                        AddError(
                            ErrorCode.FunctionImportNonNullableParametersNotAllowed,
                            EdmSchemaErrorSeverity.Error,
                            Strings.FunctionImportNonNullableParametersNotAllowed(FQName));
                    }
                }
            }
        }

        private void ValidateFunctionImportReturnType(
            SchemaElement owner, SchemaType returnType, CollectionKind returnTypeCollectionKind, EntityContainerEntitySet entitySet,
            bool entitySetPathDefined)
        {
            if (returnType is not null
                && !ReturnTypeMeetsFunctionImportBasicRequirements(returnType, returnTypeCollectionKind))
            {
                owner.AddError(
                    ErrorCode.FunctionImportUnsupportedReturnType,
                    EdmSchemaErrorSeverity.Error,
                    owner,
                    GetReturnTypeErrorMessage(Name)
                    );
            }
            ValidateFunctionImportReturnType(owner, returnType, entitySet, entitySetPathDefined);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool ReturnTypeMeetsFunctionImportBasicRequirements(SchemaType type, CollectionKind returnTypeCollectionKind)
        {
            if (type is ScalarType
                && returnTypeCollectionKind == CollectionKind.Bag)
            {
                return true;
            }
            if (type is SchemaEntityType
                && returnTypeCollectionKind == CollectionKind.Bag)
            {
                return true;
            }

            if (Schema.SchemaVersion
                == XmlConstants.EdmVersionForV1_1)
            {
                if (type is ScalarType
                    && returnTypeCollectionKind == CollectionKind.None)
                {
                    return true;
                }
                if (type is SchemaEntityType
                    && returnTypeCollectionKind == CollectionKind.None)
                {
                    return true;
                }
                if (type is SchemaComplexType
                    && returnTypeCollectionKind == CollectionKind.None)
                {
                    return true;
                }
                if (type is SchemaComplexType
                    && returnTypeCollectionKind == CollectionKind.Bag)
                {
                    return true;
                }
            }
            if (Schema.SchemaVersion
                >= XmlConstants.EdmVersionForV2)
            {
                if (type is SchemaComplexType
                    && returnTypeCollectionKind == CollectionKind.Bag)
                {
                    return true;
                }
            }
            if (Schema.SchemaVersion
                >= XmlConstants.EdmVersionForV3)
            {
                if (type is SchemaEnumType
                    && returnTypeCollectionKind == CollectionKind.Bag)
                {
                    return true;
                }
            }

            return false;
        }

        // <summary>
        // validate the following negative scenarios:
        // ReturnType="Collection(EntityTypeA)"
        // ReturnType="Collection(EntityTypeA)" EntitySet="ESet.EType is not oftype EntityTypeA"
        // EntitySet="A"
        // ReturnType="Collection(ComplexTypeA)" EntitySet="something"
        // ReturnType="Collection(ComplexTypeA)", but the ComplexTypeA has a nested complexType property, this scenario will be handle in the runtime
        // </summary>
        private void ValidateFunctionImportReturnType(
            SchemaElement owner, SchemaType returnType, EntityContainerEntitySet entitySet, bool entitySetPathDefined)
        {
            // If entity type, verify specification of entity set and that the type is appropriate for the entity set
            var entityType = returnType as SchemaEntityType;

            if (entitySet is not null && entitySetPathDefined)
            {
                owner.AddError(
                    ErrorCode.FunctionImportEntitySetAndEntitySetPathDeclared,
                    EdmSchemaErrorSeverity.Error,
                    Strings.FunctionImportEntitySetAndEntitySetPathDeclared(FQName));
            }

            if (null != entityType)
            {
                // entity type
                if (null == entitySet)
                {
                    // ReturnType="Collection(EntityTypeA)"
                    owner.AddError(
                        ErrorCode.FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet,
                        EdmSchemaErrorSeverity.Error,
                        Strings.FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet(FQName));
                }
                else if (null != entitySet.EntityType
                         && !entityType.IsOfType(entitySet.EntityType))
                {
                    // ReturnType="Collection(EntityTypeA)" EntitySet="ESet.EType is not oftype EntityTypeA"
                    owner.AddError(
                        ErrorCode.FunctionImportEntityTypeDoesNotMatchEntitySet,
                        EdmSchemaErrorSeverity.Error,
                        Strings.FunctionImportEntityTypeDoesNotMatchEntitySet(
                            FQName, entitySet.EntityType.FQName, entitySet.Name));
                }
            }
            else
            {
                // complex type
                var complexType = returnType as SchemaComplexType;
                if (complexType is not null)
                {
                    if (entitySet is not null || entitySetPathDefined)
                    {
                        // ReturnType="Collection(ComplexTypeA)" EntitySet="something"
                        owner.AddError(
                            ErrorCode.ComplexTypeAsReturnTypeAndDefinedEntitySet,
                            EdmSchemaErrorSeverity.Error,
                            owner.LineNumber,
                            owner.LinePosition,
                            Strings.ComplexTypeAsReturnTypeAndDefinedEntitySet(FQName, complexType.Name));
                    }
                }
                else
                {
                    Debug.Assert(
                        returnType is null || returnType is ScalarType || returnType is SchemaEnumType || returnType is Relationship,
                        "null return type, scalar return type, enum return type or relationship expected here.");

                    // scalar type or no return type
                    if (entitySet is not null || entitySetPathDefined)
                    {
                        // EntitySet="A"
                        owner.AddError(
                            ErrorCode.FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType,
                            EdmSchemaErrorSeverity.Error,
                            Strings.FunctionImportSpecifiesEntitySetButNotEntityType(FQName));
                    }
                }
            }
        }

        private string GetReturnTypeErrorMessage(string functionName)
        {
            string errorMessage;
            if (Schema.SchemaVersion
                == XmlConstants.EdmVersionForV1)
            {
                errorMessage = Strings.FunctionImportWithUnsupportedReturnTypeV1(functionName);
            }
            else if (Schema.SchemaVersion
                     == XmlConstants.EdmVersionForV1_1)
            {
                errorMessage = Strings.FunctionImportWithUnsupportedReturnTypeV1_1(functionName);
            }
            else
            {
                Debug.Assert(
                    XmlConstants.EdmVersionForV3 == XmlConstants.SchemaVersionLatest,
                    "Please update the error message accordingly");
                errorMessage = Strings.FunctionImportWithUnsupportedReturnTypeV2(functionName);
            }
            return errorMessage;
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            var function = new FunctionImportElement((EntityContainer)parentElement);
            CloneSetFunctionFields(function);
            function._container = _container;
            function._entitySet = _entitySet;
            function._unresolvedEntitySet = _unresolvedEntitySet;
            function._entitySetPathDefined = _entitySetPathDefined;
            return function;
        }
    }
}
