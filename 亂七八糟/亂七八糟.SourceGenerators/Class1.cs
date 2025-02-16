using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace 亂七八糟.SourceGenerators;

[Generator]
public class INotifyPropertyChangedGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ClassSyntaxReceiver receiver)
            return;

        foreach (var classDeclaration in receiver.Classes)
        {
            var className = classDeclaration.Identifier.Text;
            var sourceCode = GenerateINotifyPropertyChangedImplementation(className);
            context.AddSource($"{className}_INotifyPropertyChanged.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private string GenerateINotifyPropertyChangedImplementation(string className)
    {
        return $@"
using System.ComponentModel;

public partial class {className} : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {{
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }}
}}";
    }
}

class ClassSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            Classes.Add(classDeclaration);
        }
    }
}