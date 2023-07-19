using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpeChatAI.Generics;

/// <summary>Provides logic for generic types.</summary>
public static class GenericLogic
{
    /// <summary>Finds a generic match for the given <paramref name="search"/>
    /// with the given <paramref name="genericDefinitionToFind"/>.</summary>
    /// <param name="search">The type to search for a generic match.</param>
    /// <param name="genericDefinitionToFind">The generic definition to find
    /// a match for.</param>
    /// <returns><para>Returns <see langword="null"/> if no match was found.
    /// </para><para>Returns a <see cref="GenericTypeMatchInformation"/> if a match
    /// was found.</para></returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="search"/>
    /// or <paramref name="genericDefinitionToFind"/> is null.</exception>
    /// <exception cref="ArgumentException"><para><paramref name="genericDefinitionToFind"/>
    /// is not a generic type definition.</para>
    /// <paramref name="search"/> is a generic type definition.</exception>
    public static GenericTypeMatchInformation? GetGenericMatchFor
        (this Type search, Type genericDefinitionToFind)
    {
        if (search == null)
            throw new ArgumentNullException(nameof(search));
        if (genericDefinitionToFind == null)
            throw new ArgumentNullException(nameof(genericDefinitionToFind));
        if (!genericDefinitionToFind.IsGenericTypeDefinition)
            throw new ArgumentException
                ($"'{nameof(genericDefinitionToFind)}' is not a generic type definition."
                , nameof(genericDefinitionToFind));
        if (search.IsGenericType
            && search.IsGenericTypeDefinition
            && search == genericDefinitionToFind)
            return new GenericTypeMatchInformation
                   (  search
                   , GenericTypeMatchOrigin.OriginalType
                   , search.GetGenericArguments());

        return GetGenericMatchForInternal(search, search, search.GetInterfaces(), genericDefinitionToFind);
    }

    #region Ancillary methods for GetGenericMatchFor
    private static GenericTypeMatchInformation? GetGenericMatchForInternal(Type searchType, Type? currentType, Type[]? currentInterfaceTypes, Type genericDefinitionToFind)
    {
        if (currentType == null || currentInterfaceTypes == null)
            return null;

        if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericDefinitionToFind)
            return new GenericTypeMatchInformation(currentType, searchType == currentType
                ? GenericTypeMatchOrigin.OriginalType
                : GenericTypeMatchOrigin.BaseType, currentType.GetGenericArguments());

        var currentBaseType = currentType.BaseType;

        Type[]? baseInterfaceTypes = null;
        if (currentBaseType != null)
        {
            baseInterfaceTypes = currentBaseType.GetInterfaces();
            currentInterfaceTypes = currentInterfaceTypes.Except(baseInterfaceTypes).ToArray();
        }

        if (GetAnyInterfaceTypeGenericMatch(currentInterfaceTypes, genericDefinitionToFind) is GenericTypeMatchInformation match)
            return match;

        return GetGenericMatchForInternal(searchType, currentBaseType, baseInterfaceTypes, genericDefinitionToFind);
    }


    private static GenericTypeMatchInformation? GetAnyInterfaceTypeGenericMatch
        ( Type[] interfaces, Type genericDefinitionToFind)
    {
        for (int i = 0; i < interfaces.Length; i++)
            if (GetInterfaceTypeGenericMatch(interfaces[i], genericDefinitionToFind)
                is GenericTypeMatchInformation match)
                return match;
        return null;
    }

    private static GenericTypeMatchInformation? GetInterfaceTypeGenericMatch
        ( Type @interface, Type genericDefinitionToFind)
    {
        // This is okay because the interface's inheritance tree
        // will be replicated in the implementing type's inheritance tree.
        if (!@interface.IsGenericType)
            return null;

        var genericDef = @interface.GetGenericTypeDefinition();
        if (genericDef == genericDefinitionToFind)
            return new GenericTypeMatchInformation
                    ( @interface
                    , GenericTypeMatchOrigin.ImplementedInterface
                    , @interface.GetGenericArguments());
        return null;
    }
    #endregion Ancillary methods for GetGenericMatchFor
}
/// <summary>Contains information about a generic type
/// match.</summary>
public class GenericTypeMatchInformation
    : List<Type>
{
    /// <summary>The generic construction that was matched.</summary>
    public Type GenericConstruction { get; }
    /// <summary>The origin of the generic type match.</summary>
    public GenericTypeMatchOrigin Origin { get; }

    /// <summary>Creates a new instance of <see cref="GenericTypeMatchInformation"/>
    /// with the given <paramref name="genericConstruction"/>, <paramref name="origin"/>
    /// and <paramref name="genericArguments"/>.</summary>
    /// <param name="genericConstruction">The generic construction that was matched.</param>
    /// <param name="origin">The origin of the generic type match.</param>
    /// <param name="genericArguments">The generic arguments that were used to match
    /// the generic construction.</param>
    public GenericTypeMatchInformation
        ( Type genericConstruction
        , GenericTypeMatchOrigin origin
        , IEnumerable<Type> genericArguments)
        : base(genericArguments)
    {
        this.Origin = origin;
        this.GenericConstruction = genericConstruction;
    }
}
/// <summary>Specifies the origin of a generic type match.</summary>
public enum GenericTypeMatchOrigin
{
    /// <summary>The generic type match was found on the original
    /// type.</summary>
    OriginalType,
    /// <summary>The generic type match was found on one of the base
    /// types.</summary>
    BaseType,
    /// <summary>The generic type match was found on one of the
    /// implemented interfaces.</summary>
    ImplementedInterface,
}