using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StepGenerator
{
    [Generator]
    public class StepIncrementalGenerator : IIncrementalGenerator
    {
        private readonly string TemplateClass = "global::TheBlockWar.Scripts.Games.Simulations.Simulator";
        
        private static readonly GeneratorConfig DefaultConfig = new(
            ViewRunParameters: "global::TheBlockWar.Scripts.Views.GameView view, global::TheBlockWar.Scripts.Games.Simulations.ReadOnlySimObject sim",
            ViewDrawParameters: "global::TheBlockWar.Scripts.Views.GameView view, global::TheBlockWar.Scripts.Games.Simulations.ReadOnlySimObject sim",
            ViewRunArguments: "view, sim, this",
            ViewDrawArguments: "view, sim, this",
            ViewCoreRunParameters: "global::TheBlockWar.Scripts.Views.GameView view, global::TheBlockWar.Scripts.Games.Simulations.ReadOnlySimObject sim, {ResultType} result",
            ViewCoreDrawParameters: "global::TheBlockWar.Scripts.Views.GameView view, global::TheBlockWar.Scripts.Games.Simulations.ReadOnlySimObject sim, {ResultType} result",
            ViewCoreRunArguments: "",
            ViewCoreDrawArguments: ""
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var stepMethodsProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is MethodDeclarationSyntax method &&
                        method.AttributeLists.Count > 0,
                    transform: static (ctx, ct) => GetStepMethodInfo(ctx, ct))
                .Where(static m => m.HasValue)
                .Select(static (m, _) => m!.Value)
                .Collect();

            context.RegisterSourceOutput(stepMethodsProvider, (spc, methods) =>
            {
                Execute(spc, methods, DefaultConfig);
            });
        }

        private static StepMethodInfo? GetStepMethodInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var methodDecl = (MethodDeclarationSyntax)context.Node;

            foreach (var attrList in methodDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrName = GetSimpleName(attr.Name);
                    if (attrName is not ("Step" or "StepAttribute"))
                        continue;

                    var extraParams = ImmutableArray.CreateBuilder<StepExtraParameter>();

                    if (attr.ArgumentList != null && attr.ArgumentList.Arguments.Count >= 2)
                    {
                        var namesArg = attr.ArgumentList.Arguments[0].Expression;
                        var typesArg = attr.ArgumentList.Arguments[1].Expression;

                        var names = ExtractStringArray(namesArg);
                        var types = ExtractTypeArray(typesArg, context.SemanticModel);

                        int count = Math.Min(names.Length, types.Length);
                        for (int i = 0; i < count; i++)
                        {
                            extraParams.Add(new StepExtraParameter(names[i], types[i]));
                        }
                    }

                    var methodParams = ImmutableArray.CreateBuilder<MethodParameter>();
                    foreach (var param in methodDecl.ParameterList.Parameters)
                    {
                        var typeSyntax = param.Type;
                        var typeName = typeSyntax != null
                            ? GetGlobalTypeName(typeSyntax, context.SemanticModel)
                            : "global::System.Object";
                        var isNullable = IsNullableType(typeSyntax, context.SemanticModel);
                        var paramName = param.Identifier.Text;
                        methodParams.Add(new MethodParameter(typeName, paramName, isNullable));
                    }

                    var ns = GetNamespace(methodDecl);
                    var className = GetEnclosingClassName(methodDecl);
                    var methodName = methodDecl.Identifier.Text;

                    return new StepMethodInfo(
                        Namespace: ns,
                        ClassName: className,
                        MethodName: methodName,
                        StepClassName: $"{methodName}Step",
                        ResultClassName: $"{methodName}Result",
                        MethodParameters: methodParams.ToImmutable(),
                        ExtraParameters: extraParams.ToImmutable()
                    );
                }
            }

            return null;
        }

        private static string GetSimpleName(NameSyntax name)
        {
            return name switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                QualifiedNameSyntax qn => qn.Right.Identifier.Text,
                AliasQualifiedNameSyntax aq => aq.Name.Identifier.Text,
                _ => name.ToString()
            };
        }

        private static string[] ExtractStringArray(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case ImplicitArrayCreationExpressionSyntax implicitArray:
                    return implicitArray.Initializer.Expressions
                        .Select(e => e is LiteralExpressionSyntax lit ? lit.Token.ValueText : e.ToString())
                        .ToArray();

                case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer != null:
                    return arrayCreation.Initializer.Expressions
                        .Select(e => e is LiteralExpressionSyntax lit ? lit.Token.ValueText : e.ToString())
                        .ToArray();

                case CollectionExpressionSyntax collection:
                    return collection.Elements
                        .OfType<ExpressionElementSyntax>()
                        .Select(e => e.Expression is LiteralExpressionSyntax lit ? lit.Token.ValueText : e.Expression.ToString())
                        .ToArray();
            }

            return Array.Empty<string>();
        }

        private static string[] ExtractTypeArray(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            switch (expression)
            {
                case ImplicitArrayCreationExpressionSyntax implicitArray:
                    return implicitArray.Initializer.Expressions
                        .Select(e => GetTypeFromExpression(e, semanticModel))
                        .ToArray();

                case ArrayCreationExpressionSyntax arrayCreation when arrayCreation.Initializer != null:
                    return arrayCreation.Initializer.Expressions
                        .Select(e => GetTypeFromExpression(e, semanticModel))
                        .ToArray();

                case CollectionExpressionSyntax collection:
                    return collection.Elements
                        .OfType<ExpressionElementSyntax>()
                        .Select(e => GetTypeFromExpression(e.Expression, semanticModel))
                        .ToArray();
            }

            return Array.Empty<string>();
        }

        private static string GetTypeFromExpression(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression is TypeOfExpressionSyntax typeOf)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeOf.Type);
                var type = typeInfo.Type;
                if (type != null)
                {
                    return GetGlobalTypeName(type);
                }
                return GetGlobalTypeName(typeOf.Type, semanticModel);
            }

            return "global::System.Object";
        }

        private static string GetGlobalTypeName(TypeSyntax typeSyntax, SemanticModel semanticModel)
        {
            var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
            if (typeInfo.Type != null)
            {
                return GetGlobalTypeName(typeInfo.Type);
            }

            var text = typeSyntax.ToString();
            if (IsBuiltInType(text)) return text;
            return $"{text}";
        }

        private static string GetGlobalTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null) return "global::System.Object";

            if (typeSymbol.SpecialType != SpecialType.None)
            {
                return typeSymbol.SpecialType switch
                {
                    SpecialType.System_Object => "object",
                    SpecialType.System_String => "string",
                    SpecialType.System_Int32 => "int",
                    SpecialType.System_Int64 => "long",
                    SpecialType.System_Boolean => "bool",
                    SpecialType.System_Single => "float",
                    SpecialType.System_Double => "double",
                    SpecialType.System_Decimal => "decimal",
                    SpecialType.System_Byte => "byte",
                    SpecialType.System_SByte => "sbyte",
                    SpecialType.System_Char => "char",
                    SpecialType.System_UInt32 => "uint",
                    SpecialType.System_UInt64 => "ulong",
                    SpecialType.System_Int16 => "short",
                    SpecialType.System_UInt16 => "ushort",
                    _ => $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"
                };
            }

            return $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
        }

        private static bool IsBuiltInType(string typeName)
        {
            return typeName switch
            {
                "object" or "string" or "int" or "long" or "bool" or "float" or "double"
                    or "decimal" or "byte" or "sbyte" or "char" or "uint" or "ulong"
                    or "short" or "ushort" or "void" => true,
                _ => false
            };
        }

        private static bool IsNullableType(TypeSyntax? typeSyntax, SemanticModel semanticModel)
        {
            if (typeSyntax == null) return true; // 无类型声明视为可空

            // 检查是否有 ? 标记（NullableTypeSyntax）
            if (typeSyntax is NullableTypeSyntax) return true;

            var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
            var type = typeInfo.Type;

            if (type == null) return true;

            // 值类型不可空（除非显式 Nullable<T>，但上面 NullableTypeSyntax 已处理）
            if (type.IsValueType) return false;

            // 引用类型默认可空
            return true;
        }

        private static string GetNamespace(MethodDeclarationSyntax method)
        {
            var current = method.Parent;
            while (current != null)
            {
                if (current is NamespaceDeclarationSyntax ns)
                    return ns.Name.ToString();
                if (current is FileScopedNamespaceDeclarationSyntax fileNs)
                    return fileNs.Name.ToString();
                current = current.Parent;
            }
            return string.Empty;
        }

        private static string GetEnclosingClassName(MethodDeclarationSyntax method)
        {
            var parent = method.Parent as TypeDeclarationSyntax;
            return parent?.Identifier.Text ?? "Unknown";
        }

        private static string ReplacePlaceholders(string template, string resultTypeReplacement)
        {
            if (string.IsNullOrEmpty(template)) return template;
            return template.Replace("{ResultType}", resultTypeReplacement);
        }

        private void Execute(SourceProductionContext context, ImmutableArray<StepMethodInfo> methods, GeneratorConfig config)
        {
            if (methods.IsEmpty) return;

            var namespaceGroups = methods.GroupBy(m => m.Namespace).ToList();

            foreach (var group in namespaceGroups)
            {
                var sb = new StringBuilder();

                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("#nullable enable");
                sb.AppendLine("#pragma warning disable");
                sb.AppendLine("using GodotTask;  // GDTask 使用 using，不添加 global::");
                sb.AppendLine();

                var indent = "";
                if (!string.IsNullOrEmpty(group.Key))
                {
                    sb.AppendLine($"namespace {group.Key}");
                    sb.AppendLine("{");
                    indent = "    ";
                }

                GenerateStepAttribute(sb, indent);
                GenerateApplyStep(sb, indent, group.ToList());
                GenerateSimResult(sb, indent, group.ToList(), config);
                GenerateStepResultViewCore(sb, indent, group.ToList(), config);

                foreach (var method in group)
                {
                    GenerateApplyStepClass(sb, indent, method);
                    GenerateApplyResultClass(sb, indent, method, config);
                }

                if (!string.IsNullOrEmpty(group.Key))
                {
                    sb.AppendLine("}");
                }

                var fileName = string.IsNullOrEmpty(group.Key)
                    ? $"StepGenerated.g.cs"
                    : $"{group.Key.Replace('.', '_')}_StepGenerated.g.cs";

                context.AddSource(fileName, sb.ToString());
            }
        }

        private void GenerateStepAttribute(StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}[global::System.AttributeUsage(global::System.AttributeTargets.Method)]");
            sb.AppendLine($"{indent}public sealed class StepAttribute : global::System.Attribute");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    public string[] ParamNames {{ get; }}");
            sb.AppendLine($"{indent}    public global::System.Type[] ParamTypes {{ get; }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public StepAttribute(string[] paramNames, global::System.Type[] paramTypes)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        ParamNames = paramNames;");
            sb.AppendLine($"{indent}        ParamTypes = paramTypes;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        private void GenerateApplyStep(StringBuilder sb, string indent, List<StepMethodInfo> methods)
        {
            foreach (var method in methods)
            {
                sb.AppendLine($"{indent}[global::System.Text.Json.Serialization.JsonDerivedType(typeof({method.StepClassName}), \"{method.StepClassName}\")]");
            }

            sb.AppendLine($"{indent}public class ApplyStep");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    public virtual StepResult Apply(global::TheBlockWar.Scripts.Games.Simulations.SimObject obj) => new StepResult(short.MinValue, -1);");
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        private void GenerateSimResult(StringBuilder sb, string indent, List<StepMethodInfo> methods, GeneratorConfig config)
        {
            foreach (var method in methods)
            {
                sb.AppendLine($"{indent}[global::System.Text.Json.Serialization.JsonDerivedType(typeof({method.ResultClassName}), \"{method.ResultClassName}\")]");
            }

            var viewRunParams = ReplacePlaceholders(config.ViewRunParameters, "StepResult");
            var viewDrawParams = ReplacePlaceholders(config.ViewDrawParameters, "StepResult");
            var viewRunSig = string.IsNullOrEmpty(viewRunParams) ? "()" : $"({viewRunParams})";
            var viewDrawSig = string.IsNullOrEmpty(viewDrawParams) ? "()" : $"({viewDrawParams})";

            sb.Append(
                $$"""
                 {{indent}}public class StepResult(short deep, int team)
                 {{indent}}{
                 {{indent}}    public short Deep { get; set; } = deep;
                 {{indent}}    public int Team { get; set; } = team;
                 {{indent}}    public virtual ApplyStep GetStep() => new ApplyStep();
                 {{indent}}    public virtual GDTask ViewRun{{viewRunSig}} => GDTask.CompletedTask;
                 {{indent}}    public virtual void ViewDraw{{viewDrawSig}} { }
                 {{indent}}}
                 """);
            sb.AppendLine();
        }

        private void GenerateStepResultViewCore(StringBuilder sb, string indent, List<StepMethodInfo> methods, GeneratorConfig config)
        {
            var coreRunParamsTemplate = !string.IsNullOrEmpty(config.ViewCoreRunParameters)
                ? config.ViewCoreRunParameters
                : config.ViewRunParameters;
            var coreDrawParamsTemplate = !string.IsNullOrEmpty(config.ViewCoreDrawParameters)
                ? config.ViewCoreDrawParameters
                : config.ViewDrawParameters;

            sb.AppendLine($"{indent}partial class Simulator");
            sb.AppendLine($"{indent}{{");

            foreach (var method in methods)
            {
                var viewRunParams = ReplacePlaceholders(coreRunParamsTemplate, method.ResultClassName);
                var viewDrawParams = ReplacePlaceholders(coreDrawParamsTemplate, method.ResultClassName);
                var viewRunSig = string.IsNullOrEmpty(viewRunParams) ? "()" : $"({viewRunParams})";
                var viewDrawSig = string.IsNullOrEmpty(viewDrawParams) ? "()" : $"({viewDrawParams})";

                sb.AppendLine($"{indent}    public static partial GDTask {method.MethodName}ViewRun{viewRunSig};");
                sb.AppendLine($"{indent}    public static partial void {method.MethodName}ViewDraw{viewDrawSig};");
            }

            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        private void GenerateApplyStepClass(StringBuilder sb, string indent, StepMethodInfo method)
        {
            var ctorParams = string.Join(", ", method.MethodParameters.Select(p => $"{GetNullableTypeName(p)} {p.ParameterName}"));
            var callArgs = string.Join(", ", method.MethodParameters.Select(p => Capitalize(p.ParameterName)));

            sb.AppendLine($"{indent}public class {method.StepClassName}({ctorParams}) : ApplyStep");
            sb.AppendLine($"{indent}{{");

            foreach (var param in method.MethodParameters)
            {
                var typeName = GetNullableTypeName(param);
                sb.AppendLine($"{indent}    public {typeName} {Capitalize(param.ParameterName)} {{ get; set; }} = {param.ParameterName};");
            }

            sb.AppendLine();
            sb.AppendLine($"{indent}    public override {method.ResultClassName} Apply(global::TheBlockWar.Scripts.Games.Simulations.SimObject obj)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return obj.{method.MethodName}({callArgs});");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override string ToString()");
            sb.AppendLine($"{indent}    {{");
            var stepPropPatterns = method.MethodParameters.Select(p => $"{Capitalize(p.ParameterName)}: {{{Capitalize(p.ParameterName)}}}");
            sb.AppendLine($"{indent}        return $\"{{{{{method.StepClassName}->{string.Join(", ", stepPropPatterns)}}}}}\";");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        private void GenerateApplyResultClass(StringBuilder sb, string indent, StepMethodInfo method, GeneratorConfig config)
        {
            var allCtorParams = new List<string> { "short deep, int team" };
            allCtorParams.AddRange(method.MethodParameters.Select(p => $"{GetNullableTypeName(p)} {p.ParameterName}"));
            allCtorParams.AddRange(method.ExtraParameters.Select(e => $"{e.TypeName} {e.Name}"));

            var ctorParamList = string.Join(", ", allCtorParams);
            var baseCall = "StepResult(deep, team)";

            var viewRunParams = ReplacePlaceholders(config.ViewRunParameters, "StepResult");
            var viewDrawParams = ReplacePlaceholders(config.ViewDrawParameters, "StepResult");
            var viewRunSig = string.IsNullOrEmpty(viewRunParams) ? "()" : $"({viewRunParams})";
            var viewDrawSig = string.IsNullOrEmpty(viewDrawParams) ? "()" : $"({viewDrawParams})";

            var coreRunArgsTemplate = !string.IsNullOrEmpty(config.ViewCoreRunArguments)
                ? config.ViewCoreRunArguments
                : config.ViewRunArguments;
            var coreDrawArgsTemplate = !string.IsNullOrEmpty(config.ViewCoreDrawArguments)
                ? config.ViewCoreDrawArguments
                : config.ViewDrawArguments;

            var viewRunCall = string.IsNullOrEmpty(coreRunArgsTemplate) ? "()" : $"({coreRunArgsTemplate})";
            var viewDrawCall = string.IsNullOrEmpty(coreDrawArgsTemplate) ? "()" : $"({coreDrawArgsTemplate})";

            sb.AppendLine($"{indent}public class {method.ResultClassName}({ctorParamList}) : {baseCall}");
            sb.AppendLine($"{indent}{{");

            foreach (var param in method.MethodParameters)
            {
                var typeName = GetNullableTypeName(param);
                sb.AppendLine($"{indent}    public {typeName} {Capitalize(param.ParameterName)} {{ get; set; }} = {param.ParameterName};");
            }

            foreach (var extra in method.ExtraParameters)
            {
                sb.AppendLine($"{indent}    public {extra.TypeName} Re{Capitalize(extra.Name)} {{ get; set; }} = {extra.Name};");
            }

            var stepCtorArgs = string.Join(", ", method.MethodParameters.Select(p => $"{Capitalize(p.ParameterName)}"));
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override {method.StepClassName} GetStep() => new {method.StepClassName}({stepCtorArgs});");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override GDTask ViewRun{viewRunSig} => {TemplateClass}.{method.MethodName}ViewRun{viewRunCall};");
            sb.AppendLine($"{indent}    public override void ViewDraw{viewDrawSig} => {TemplateClass}.{method.MethodName}ViewDraw{viewDrawCall};");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override string ToString()");
            sb.AppendLine($"{indent}    {{");
            string[] basePatterns = ["Deep: {Deep}", "Team: {Team}"];
            var stepPropPatterns = method.MethodParameters.Select(p => $"{Capitalize(p.ParameterName)}: {{{Capitalize(p.ParameterName)}}}");
            var resultPropPatterns = method.ExtraParameters.Select(e => $"Re{Capitalize(e.Name)}: {{Re{Capitalize(e.Name)}}}");
            var allPatterns = basePatterns.Concat(stepPropPatterns).Concat(resultPropPatterns);
            sb.AppendLine($"{indent}        return $\"{{{{{method.ResultClassName}->{string.Join(", ", allPatterns)}}}}}\";");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        private static string GetNullableTypeName(MethodParameter param)
        {
            return param.IsNullable ? $"{param.TypeName}?" : param.TypeName;
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }

    internal readonly record struct StepMethodInfo(
        string Namespace,
        string ClassName,
        string MethodName,
        string StepClassName,
        string ResultClassName,
        ImmutableArray<MethodParameter> MethodParameters,
        ImmutableArray<StepExtraParameter> ExtraParameters
    );

    internal readonly record struct MethodParameter(string TypeName, string ParameterName, bool IsNullable);
    internal readonly record struct StepExtraParameter(string Name, string TypeName);

    internal readonly record struct GeneratorConfig(
        string ViewRunParameters,
        string ViewDrawParameters,
        string ViewRunArguments,
        string ViewDrawArguments,
        string ViewCoreRunParameters,
        string ViewCoreDrawParameters,
        string ViewCoreRunArguments,
        string ViewCoreDrawArguments
    );
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {
    }
}