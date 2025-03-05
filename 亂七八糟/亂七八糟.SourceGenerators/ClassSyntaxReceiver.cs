namespace 亂七八糟.SourceGenerators
{
    public class ClassSyntaxReceiver : Microsoft.CodeAnalysis.ISyntaxReceiver
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