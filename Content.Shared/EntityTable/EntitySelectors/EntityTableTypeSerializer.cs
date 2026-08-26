using Content.Shared._ES.EntityTable.EntitySelectors;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared.EntityTable.EntitySelectors;

[TypeSerializer]
public sealed class EntityTableTypeSerializer :
    ITypeReader<EntityTableSelector, MappingDataNode>
{
    public ValidationNode Validate(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        if (node.Has(EntSelector.IdDataFieldTag))
            return serializationManager.ValidateNode<EntSelector>(node, context);

        if (node.Has(ESAllSelector.DataFieldTag))
            return serializationManager.ValidateNode<ESAllSelector>(node, context);
        if (node.Has(ESGroupSelector.DataFieldTag))
            return serializationManager.ValidateNode<ESGroupSelector>(node, context);
        if (node.Has(ESNestedSelector.DataFieldTag))
            return serializationManager.ValidateNode<ESNestedSelector>(node, context);
        if (node.Has(ESPickSelector.DataFieldTag))
            return serializationManager.ValidateNode<ESPickSelector>(node, context);

        return new ErrorNode(node, "Custom validation not supported! Please specify the type manually!");
    }

    public EntityTableSelector Read(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<EntityTableSelector>? instanceProvider = null)
    {
        if (node.Has(EntSelector.IdDataFieldTag))
            return serializationManager.Read<EntSelector>(node, context, notNullableOverride: true);

        if (node.Has(ESAllSelector.DataFieldTag))
            return serializationManager.Read<ESAllSelector>(node, context, notNullableOverride: true);
        if (node.Has(ESGroupSelector.DataFieldTag))
            return serializationManager.Read<ESGroupSelector>(node, context, notNullableOverride: true);
        if (node.Has(ESNestedSelector.DataFieldTag))
            return serializationManager.Read<ESNestedSelector>(node, context, notNullableOverride: true);
        if (node.Has(ESPickSelector.DataFieldTag))
            return serializationManager.Read<ESPickSelector>(node, context, notNullableOverride: true);

        return serializationManager.Read<EntityTableSelector>(node, context, notNullableOverride: true);
    }
}
