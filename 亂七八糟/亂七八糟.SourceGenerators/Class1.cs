namespace 亂七八糟.SourceGenerators
{
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