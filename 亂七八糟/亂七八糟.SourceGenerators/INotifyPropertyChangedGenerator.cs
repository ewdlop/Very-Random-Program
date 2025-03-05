
using Microsoft.CodeAnalysis;

namespace 亂七八糟.SourceGenerators
{
#if BANNED
    //CS5035 GeneratorInitializationContext 
    [Microsoft.CodeAnalysis.Generator]
    public class INotifyPropertyChangedGenerator : Microsoft.CodeAnalysis.ISourceGenerator
    {
        public void Initialize(Microsoft.CodeAnalysis.GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
        }

        public void Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is ClassSyntaxReceiver receiver))
                return;

            foreach (var classDeclaration in receiver.Classes)
            {
                var className = classDeclaration.Identifier.Text;
                var sourceCode = GenerateINotifyPropertyChangedImplementation(className);
                context.AddSource($"{className}_INotifyPropertyChanged.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sourceCode, System.Text.Encoding.UTF8));
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

    class ClassSyntaxReceiver : Microsoft.CodeAnalysis.ISyntaxReceiver
    {
        public System.Collections.Generic.List<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax> Classes { get; } = new System.Collections.Generic.List<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(Microsoft.CodeAnalysis.SyntaxNode syntaxNode)
        {
            if (syntaxNode is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDeclaration)
            {
                Classes.Add(classDeclaration);
            }
        }
    }
}
#endif

    [Microsoft.CodeAnalysis.Generator]
    public class INotifyPropertyChangedGenerator : Microsoft.CodeAnalysis.IIncrementalGenerator
    {
        public void Initialize(Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
             .CreateSyntaxProvider(
                 predicate: (syntaxNode, _) => IsClassDeclaration(syntaxNode),
                 transform: (ctx, _) => GetClassDeclaration(ctx))
             .Where(m => m != null);

            //var compilation = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(classDeclarations, (spc, classDeclaration) =>
            {
                var className = classDeclaration.Identifier.Text;
                var sourceCode = GenerateINotifyPropertyChangedImplementation(className);
                spc.AddSource($"{className}_INotifyPropertyChanged.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(sourceCode, System.Text.Encoding.UTF8));
            });
        }

        private static bool IsClassDeclaration(Microsoft.CodeAnalysis.SyntaxNode syntaxNode)
        {
            return syntaxNode is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
        }

        private static Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax GetClassDeclaration(Microsoft.CodeAnalysis.GeneratorSyntaxContext context)
        {
            return (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)context.Node;
        }

        private string GenerateINotifyPropertyChangedImplementation(string className)
        {
            return $@"
using SourceGenerators;
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
}