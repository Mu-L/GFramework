using GFramework.SourceGenerators.Common.Constants;

namespace GFramework.SourceGenerators.Cqrs;

/// <summary>
///     为当前编译程序集生成 CQRS 处理器注册器，以减少运行时的程序集反射扫描成本。
/// </summary>
[Generator]
public sealed class CqrsHandlerRegistryGenerator : IIncrementalGenerator
{
    private const string CqrsContractsNamespace = $"{PathContests.CqrsAbstractionsNamespace}.Cqrs";
    private const string CqrsRuntimeNamespace = PathContests.CqrsNamespace;
    private const string LoggingNamespace = $"{PathContests.CoreAbstractionsNamespace}.Logging";
    private const string IRequestHandlerMetadataName = $"{CqrsContractsNamespace}.IRequestHandler`2";
    private const string INotificationHandlerMetadataName = $"{CqrsContractsNamespace}.INotificationHandler`1";
    private const string IStreamRequestHandlerMetadataName = $"{CqrsContractsNamespace}.IStreamRequestHandler`2";
    private const string ICqrsHandlerRegistryMetadataName = $"{CqrsRuntimeNamespace}.ICqrsHandlerRegistry";

    private const string CqrsReflectionFallbackAttributeMetadataName =
        $"{CqrsRuntimeNamespace}.CqrsReflectionFallbackAttribute";

    private const string CqrsHandlerRegistryAttributeMetadataName =
        $"{CqrsRuntimeNamespace}.CqrsHandlerRegistryAttribute";

    private const string ILoggerMetadataName = $"{LoggingNamespace}.ILogger";
    private const string IServiceCollectionMetadataName = "Microsoft.Extensions.DependencyInjection.IServiceCollection";
    private const string GeneratedNamespace = "GFramework.Generated.Cqrs";
    private const string GeneratedTypeName = "__GFrameworkGeneratedCqrsHandlerRegistry";
    private const string HintName = "CqrsHandlerRegistry.g.cs";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var generationEnvironment = context.CompilationProvider
            .Select(static (compilation, _) => CreateGenerationEnvironment(compilation));

        // Restrict semantic analysis to type declarations that can actually contribute implemented interfaces.
        var handlerCandidates = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsHandlerCandidate(node),
                static (syntaxContext, _) => TransformHandlerCandidate(syntaxContext))
            .Where(static candidate => candidate is not null)
            .Collect();

        context.RegisterSourceOutput(
            generationEnvironment.Combine(handlerCandidates),
            static (productionContext, pair) => Execute(productionContext, pair.Left, pair.Right));
    }

    private static GenerationEnvironment CreateGenerationEnvironment(Compilation compilation)
    {
        var generationEnabled = compilation.GetTypeByMetadataName(IRequestHandlerMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(INotificationHandlerMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(IStreamRequestHandlerMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(ICqrsHandlerRegistryMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(
                                    CqrsHandlerRegistryAttributeMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(ILoggerMetadataName) is not null &&
                                compilation.GetTypeByMetadataName(IServiceCollectionMetadataName) is not null;

        return new GenerationEnvironment(
            generationEnabled,
            GetReflectionFallbackEmissionMode(
                compilation.GetTypeByMetadataName(CqrsReflectionFallbackAttributeMetadataName)));
    }

    private static bool IsHandlerCandidate(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax
        {
            BaseList.Types.Count: > 0
        };
    }

    private static HandlerCandidateAnalysis? TransformHandlerCandidate(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol type)
            return null;

        if (!IsConcreteHandlerType(type))
            return null;

        var handlerInterfaces = type.AllInterfaces
            .Where(IsSupportedHandlerInterface)
            .OrderBy(GetTypeSortKey, StringComparer.Ordinal)
            .ToImmutableArray();

        if (handlerInterfaces.IsDefaultOrEmpty)
            return null;

        var implementationTypeDisplayName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (!CanReferenceFromGeneratedRegistry(type) ||
            handlerInterfaces.Any(interfaceType => !CanReferenceFromGeneratedRegistry(interfaceType)))
        {
            return new HandlerCandidateAnalysis(
                implementationTypeDisplayName,
                ImmutableArray<HandlerRegistrationSpec>.Empty,
                true,
                GetReflectionFallbackTypeName(type));
        }

        var implementationLogName = GetLogDisplayName(type);
        var registrations = ImmutableArray.CreateBuilder<HandlerRegistrationSpec>(handlerInterfaces.Length);
        foreach (var handlerInterface in handlerInterfaces)
        {
            registrations.Add(new HandlerRegistrationSpec(
                handlerInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                implementationTypeDisplayName,
                GetLogDisplayName(handlerInterface),
                implementationLogName));
        }

        return new HandlerCandidateAnalysis(
            implementationTypeDisplayName,
            registrations.MoveToImmutable(),
            false,
            null);
    }

    private static void Execute(SourceProductionContext context, GenerationEnvironment generationEnvironment,
        ImmutableArray<HandlerCandidateAnalysis?> candidates)
    {
        if (!generationEnvironment.GenerationEnabled)
            return;

        var registrations = CollectRegistrations(
            candidates,
            out var hasUnsupportedConcreteHandler,
            out var reflectionFallbackTypeNames);

        if (registrations.Count == 0)
            return;

        // If the runtime contract does not yet expose the reflection fallback marker,
        // keep the previous all-or-nothing behavior so unsupported handlers are not silently dropped.
        if (hasUnsupportedConcreteHandler &&
            generationEnvironment.ReflectionFallbackEmissionMode == ReflectionFallbackEmissionMode.Disabled)
            return;

        context.AddSource(
            HintName,
            GenerateSource(
                registrations,
                hasUnsupportedConcreteHandler,
                generationEnvironment.ReflectionFallbackEmissionMode,
                reflectionFallbackTypeNames));
    }

    private static List<HandlerRegistrationSpec> CollectRegistrations(
        ImmutableArray<HandlerCandidateAnalysis?> candidates,
        out bool hasUnsupportedConcreteHandler,
        out IReadOnlyList<string> reflectionFallbackTypeNames)
    {
        var registrations = new List<HandlerRegistrationSpec>();
        hasUnsupportedConcreteHandler = false;
        var fallbackTypeNames = new SortedSet<string>(StringComparer.Ordinal);

        // Partial declarations surface the same symbol through multiple syntax nodes.
        // Collapse them by implementation type so generated registrations stay stable and duplicate-free.
        var uniqueCandidates = new Dictionary<string, HandlerCandidateAnalysis>(StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            if (candidate is null)
                continue;

            if (candidate.Value.HasUnsupportedConcreteHandler)
            {
                hasUnsupportedConcreteHandler = true;
                var reflectionFallbackTypeName = candidate.Value.ReflectionFallbackTypeName;
                if (reflectionFallbackTypeName is not null &&
                    !string.IsNullOrWhiteSpace(reflectionFallbackTypeName))
                {
                    fallbackTypeNames.Add(reflectionFallbackTypeName);
                }

                continue;
            }

            uniqueCandidates[candidate.Value.ImplementationTypeDisplayName] = candidate.Value;
        }

        foreach (var candidate in uniqueCandidates.Values)
        {
            registrations.AddRange(candidate.Registrations);
        }

        registrations.Sort(static (left, right) =>
        {
            var implementationComparison = StringComparer.Ordinal.Compare(
                left.ImplementationLogName,
                right.ImplementationLogName);

            return implementationComparison != 0
                ? implementationComparison
                : StringComparer.Ordinal.Compare(left.HandlerInterfaceLogName, right.HandlerInterfaceLogName);
        });

        reflectionFallbackTypeNames = fallbackTypeNames.ToArray();
        return registrations;
    }

    private static ReflectionFallbackEmissionMode GetReflectionFallbackEmissionMode(INamedTypeSymbol? attributeType)
    {
        if (attributeType is null)
            return ReflectionFallbackEmissionMode.Disabled;

        foreach (var constructor in attributeType.InstanceConstructors)
        {
            if (constructor.Parameters.Length != 1)
                continue;

            if (constructor.Parameters[0].Type is IArrayTypeSymbol arrayType &&
                arrayType.ElementType.SpecialType == SpecialType.System_String)
            {
                return ReflectionFallbackEmissionMode.PreciseTypeNames;
            }
        }

        return ReflectionFallbackEmissionMode.MarkerOnly;
    }

    private static bool IsConcreteHandlerType(INamedTypeSymbol type)
    {
        return type.TypeKind is TypeKind.Class or TypeKind.Struct &&
               !type.IsAbstract &&
               !ContainsGenericParameters(type);
    }

    private static bool ContainsGenericParameters(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.TypeParameters.Length > 0)
                return true;
        }

        return false;
    }

    private static bool IsSupportedHandlerInterface(INamedTypeSymbol interfaceType)
    {
        if (!interfaceType.IsGenericType)
            return false;

        var definitionMetadataName = GetFullyQualifiedMetadataName(interfaceType.OriginalDefinition);
        return string.Equals(definitionMetadataName, IRequestHandlerMetadataName, StringComparison.Ordinal) ||
               string.Equals(definitionMetadataName, INotificationHandlerMetadataName, StringComparison.Ordinal) ||
               string.Equals(definitionMetadataName, IStreamRequestHandlerMetadataName, StringComparison.Ordinal);
    }

    private static bool CanReferenceFromGeneratedRegistry(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol arrayType:
                return CanReferenceFromGeneratedRegistry(arrayType.ElementType);
            case INamedTypeSymbol namedType:
                if (!IsTypeChainAccessible(namedType))
                    return false;

                return namedType.TypeArguments.All(CanReferenceFromGeneratedRegistry);
            case IPointerTypeSymbol pointerType:
                return CanReferenceFromGeneratedRegistry(pointerType.PointedAtType);
            case ITypeParameterSymbol:
                return false;
            default:
                return true;
        }
    }

    private static bool IsTypeChainAccessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (!IsSymbolAccessible(current))
                return false;
        }

        return true;
    }

    private static bool IsSymbolAccessible(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal
            or Accessibility.ProtectedOrInternal;
    }

    private static string GetFullyQualifiedMetadataName(INamedTypeSymbol type)
    {
        var nestedTypes = new Stack<string>();
        for (var current = type; current is not null; current = current.ContainingType)
        {
            nestedTypes.Push(current.MetadataName);
        }

        var builder = new StringBuilder();
        if (!type.ContainingNamespace.IsGlobalNamespace)
        {
            builder.Append(type.ContainingNamespace.ToDisplayString());
            builder.Append('.');
        }

        while (nestedTypes.Count > 0)
        {
            builder.Append(nestedTypes.Pop());
            if (nestedTypes.Count > 0)
                builder.Append('.');
        }

        return builder.ToString();
    }

    private static string GetReflectionFallbackTypeName(INamedTypeSymbol type)
    {
        var nestedTypes = new Stack<string>();
        for (var current = type; current is not null; current = current.ContainingType)
        {
            nestedTypes.Push(current.MetadataName);
        }

        var builder = new StringBuilder();
        if (!type.ContainingNamespace.IsGlobalNamespace)
        {
            builder.Append(type.ContainingNamespace.ToDisplayString());
            builder.Append('.');
        }

        var isFirstType = true;
        while (nestedTypes.Count > 0)
        {
            if (!isFirstType)
                builder.Append('+');

            builder.Append(nestedTypes.Pop());
            isFirstType = false;
        }

        return builder.ToString();
    }

    private static string GetTypeSortKey(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string GetLogDisplayName(ITypeSymbol type)
    {
        return GetTypeSortKey(type).Replace("global::", string.Empty);
    }

    private static string GenerateSource(
        IReadOnlyList<HandlerRegistrationSpec> registrations,
        bool emitReflectionFallbackAttribute,
        ReflectionFallbackEmissionMode reflectionFallbackEmissionMode,
        IReadOnlyList<string> reflectionFallbackTypeNames)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.Append("[assembly: global::");
        builder.Append(CqrsRuntimeNamespace);
        builder.Append(".CqrsHandlerRegistryAttribute(typeof(global::");
        builder.Append(GeneratedNamespace);
        builder.Append('.');
        builder.Append(GeneratedTypeName);
        builder.AppendLine("))]");
        if (emitReflectionFallbackAttribute &&
            reflectionFallbackEmissionMode != ReflectionFallbackEmissionMode.Disabled)
        {
            AppendReflectionFallbackAttribute(builder, reflectionFallbackEmissionMode, reflectionFallbackTypeNames);
        }

        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(GeneratedNamespace);
        builder.AppendLine(";");
        builder.AppendLine();
        builder.Append("internal sealed class ");
        builder.Append(GeneratedTypeName);
        builder.Append(" : global::");
        builder.Append(CqrsRuntimeNamespace);
        builder.AppendLine(".ICqrsHandlerRegistry");
        builder.AppendLine("{");
        builder.Append(
            "    public void Register(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, global::");
        builder.Append(LoggingNamespace);
        builder.AppendLine(".ILogger logger)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (services is null)");
        builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(services));");
        builder.AppendLine("        if (logger is null)");
        builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(logger));");
        builder.AppendLine();

        foreach (var registration in registrations)
        {
            builder.AppendLine(
                "        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient(");
            builder.AppendLine("            services,");
            builder.Append("            typeof(");
            builder.Append(registration.HandlerInterfaceDisplayName);
            builder.AppendLine("),");
            builder.Append("            typeof(");
            builder.Append(registration.ImplementationTypeDisplayName);
            builder.AppendLine("));");
            builder.Append("        logger.Debug(\"Registered CQRS handler ");
            builder.Append(EscapeStringLiteral(registration.ImplementationLogName));
            builder.Append(" as ");
            builder.Append(EscapeStringLiteral(registration.HandlerInterfaceLogName));
            builder.AppendLine(".\");");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendReflectionFallbackAttribute(
        StringBuilder builder,
        ReflectionFallbackEmissionMode reflectionFallbackEmissionMode,
        IReadOnlyList<string> reflectionFallbackTypeNames)
    {
        builder.Append("[assembly: global::");
        builder.Append(CqrsRuntimeNamespace);
        builder.Append(".CqrsReflectionFallbackAttribute");

        if (reflectionFallbackEmissionMode == ReflectionFallbackEmissionMode.PreciseTypeNames &&
            reflectionFallbackTypeNames.Count > 0)
        {
            builder.Append('(');
            for (var index = 0; index < reflectionFallbackTypeNames.Count; index++)
            {
                if (index > 0)
                    builder.Append(", ");

                builder.Append('"');
                builder.Append(EscapeStringLiteral(reflectionFallbackTypeNames[index]));
                builder.Append('"');
            }

            builder.AppendLine(")]");
            return;
        }

        builder.AppendLine("()]");
    }

    private static string EscapeStringLiteral(string value)
    {
        return value.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private readonly record struct HandlerRegistrationSpec(
        string HandlerInterfaceDisplayName,
        string ImplementationTypeDisplayName,
        string HandlerInterfaceLogName,
        string ImplementationLogName);

    private readonly struct HandlerCandidateAnalysis : IEquatable<HandlerCandidateAnalysis>
    {
        public HandlerCandidateAnalysis(
            string implementationTypeDisplayName,
            ImmutableArray<HandlerRegistrationSpec> registrations,
            bool hasUnsupportedConcreteHandler,
            string? reflectionFallbackTypeName)
        {
            ImplementationTypeDisplayName = implementationTypeDisplayName;
            Registrations = registrations;
            HasUnsupportedConcreteHandler = hasUnsupportedConcreteHandler;
            ReflectionFallbackTypeName = reflectionFallbackTypeName;
        }

        public string ImplementationTypeDisplayName { get; }

        public ImmutableArray<HandlerRegistrationSpec> Registrations { get; }

        public bool HasUnsupportedConcreteHandler { get; }

        public string? ReflectionFallbackTypeName { get; }

        public bool Equals(HandlerCandidateAnalysis other)
        {
            if (!string.Equals(ImplementationTypeDisplayName, other.ImplementationTypeDisplayName,
                    StringComparison.Ordinal) ||
                HasUnsupportedConcreteHandler != other.HasUnsupportedConcreteHandler ||
                !string.Equals(ReflectionFallbackTypeName, other.ReflectionFallbackTypeName,
                    StringComparison.Ordinal) ||
                Registrations.Length != other.Registrations.Length)
            {
                return false;
            }

            for (var index = 0; index < Registrations.Length; index++)
            {
                if (!Registrations[index].Equals(other.Registrations[index]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is HandlerCandidateAnalysis other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StringComparer.Ordinal.GetHashCode(ImplementationTypeDisplayName);
                hashCode = (hashCode * 397) ^ HasUnsupportedConcreteHandler.GetHashCode();
                hashCode = (hashCode * 397) ^
                           (ReflectionFallbackTypeName is null
                               ? 0
                               : StringComparer.Ordinal.GetHashCode(ReflectionFallbackTypeName));
                foreach (var registration in Registrations)
                {
                    hashCode = (hashCode * 397) ^ registration.GetHashCode();
                }

                return hashCode;
            }
        }
    }

    private readonly record struct GenerationEnvironment(
        bool GenerationEnabled,
        ReflectionFallbackEmissionMode ReflectionFallbackEmissionMode);

    private enum ReflectionFallbackEmissionMode
    {
        Disabled,
        MarkerOnly,
        PreciseTypeNames
    }
}
