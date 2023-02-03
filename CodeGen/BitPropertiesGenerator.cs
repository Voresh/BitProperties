using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class BitPropertiesGenerator : ISourceGenerator {
    private class SyntaxReceiver : ISyntaxReceiver {
        public List<TypeDeclarationSyntax> Types { get; private set; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            try {
                if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax) {
                    if (typeDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                        foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
                            if (attributeListSyntax.Attributes.Any(_ => _.Name.ToString() == "BitProperty")) {
                                Types.Add(typeDeclarationSyntax);
                                break;
                            }
                }
            }
            catch (Exception e) {
            }
        }
    }

    public void Execute(GeneratorExecutionContext context) {
        if (context.SyntaxReceiver is SyntaxReceiver syntaxReceiver) {
            try {
                foreach (var typeDeclarationSyntax in syntaxReceiver.Types) {
                    if (typeDeclarationSyntax.Parent is TypeDeclarationSyntax)
                        continue; //nested classes are not supported
                    var b = new StringBuilder();
                    b.Append("using System;"); b.Append('\n');
                    var namespaceString 
                        = typeDeclarationSyntax.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax 
                        && !string.IsNullOrEmpty(namespaceDeclarationSyntax.Name.ToString())
                            ? namespaceDeclarationSyntax.Name.ToString() 
                            : null;
                    if (!string.IsNullOrEmpty(namespaceString)) {
                        b.Append("namespace "); b.Append(namespaceString); b.Append(" {"); b.Append('\n');
                    }
                    b.Append("  public partial class "); b.Append(typeDeclarationSyntax.Identifier); b.Append(" {"); b.Append('\n');
                    b.Append("      private byte _bitField;"); b.Append('\n');
                    foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists) {
                        foreach (var attribute in attributeListSyntax.Attributes) {
                            if (attribute.Name.ToString() != "BitProperty")
                                continue;
                            var arguments = attribute.ArgumentList;
                            if (arguments == null)
                                continue;
                            
                            var typeArgument = arguments.Arguments[0];
                            var typeString = typeArgument.Expression.NormalizeWhitespace().ToFullString();
                            typeString = typeString
                                .Replace("typeof(", string.Empty)
                                .Replace(")", string.Empty);
                            var nameArgument = arguments.Arguments[1];
                            var nameString = nameArgument.Expression.NormalizeWhitespace().ToFullString().Replace("\"", string.Empty);
                            var startIndexArgument = arguments.Arguments[2];
                            var startIndexString = startIndexArgument.Expression.NormalizeWhitespace().ToFullString();
                            var startArgumentInt = int.Parse(startIndexString);
                            var endIndexArgument = arguments.Arguments[3];
                            var endIndexString = endIndexArgument.Expression.NormalizeWhitespace().ToFullString();
                            var endArgumentInt = int.Parse(endIndexString);
                            string GetGetterBits() {
                                const int bitsAmount = 8;
                                var resultArray = new char[bitsAmount];
                                for (var i = 0; i < bitsAmount; i++)
                                {
                                    var targetIndex = bitsAmount - 1 - i;
                                    if (i < startArgumentInt)
                                    {
                                        resultArray[targetIndex] = '0';
                                        continue;
                                    }
                                    if (i >= startArgumentInt && i <= endArgumentInt)
                                    {
                                        resultArray[targetIndex] = '1';
                                        continue;
                                    }	
                                    resultArray[targetIndex] = '0';
                                }
                                return new string(resultArray);
                            }
                            (string, string) GetSetterBits() {
                                const int bitsAmount = 8;
                                var resultArray = new char[bitsAmount];
                                for (var i = 0; i < bitsAmount; i++)
                                {
                                    var targetIndex = bitsAmount - 1 - i;
                                    if (i < startArgumentInt)
                                    {
                                        resultArray[targetIndex] = '1';
                                        continue;
                                    }
                                    if (i >= startArgumentInt && i <= endArgumentInt)
                                    {
                                        resultArray[targetIndex] = '0';
                                        continue;
                                    }	
                                    resultArray[targetIndex] = '1';
                                }
                                var typeSize = endArgumentInt - startArgumentInt + 1;
                                var resultArray2 = new char[bitsAmount];
                                for (var i = 0; i < bitsAmount; i++)
                                {
                                    var targetIndex = bitsAmount - 1 - i;
                                    if (i < typeSize)
                                    {
                                        resultArray2[targetIndex] = '1';
                                        continue;
                                    }
                                    resultArray2[targetIndex] = '0';
                                }
                                return (new string(resultArray), new string(resultArray2));
                            }
                            string GetGetterTypeConversion() {
                                if (typeString == "bool")
                                    return "1 == ";
                                if (typeString == "int")
                                    return string.Empty;
                                return string.Empty;
                            }
                            string GetSetterTypeConversion() {
                                if (typeString == "bool")
                                    return "(value ? 1 : 0)";
                                if (typeString == "int")
                                    return "value";
                                return string.Empty;
                            }
                            var getterTypeConversion = GetGetterTypeConversion();
                            var setterTypeConversion = GetSetterTypeConversion();
                            b.Append("      public ");  b.Append(typeString); b.Append(" "); b.Append(nameString); b.Append(" {"); b.Append("\n");
                            var getterBits = GetGetterBits();
                            b.Append("          get => "); b.Append(getterTypeConversion); b.Append("((_bitField & 0b"); b.Append(getterBits); b.Append(") >> "); b.Append(startArgumentInt); b.Append(");"); b.Append("\n");
                            var (setterBits1, setterBits2) = GetSetterBits();
                            b.Append("          set => _bitField = (byte) ((_bitField & 0b"); b.Append(setterBits1); b.Append(") | (("); b.Append(setterTypeConversion); b.Append(" & 0b"); b.Append(setterBits2); b.Append(") << "); b.Append(startArgumentInt); b.Append("));"); b.Append("\n");
                            b.Append("      }"); b.Append("\n");
                        }
                    }
                    b.Append("  }"); b.Append('\n');
                    if (!string.IsNullOrEmpty(namespaceString))
                        b.Append("}");
                    context.AddSource($"{typeDeclarationSyntax.Identifier}_bp", SourceText.From(b.ToString(), Encoding.UTF8));
                }
            }
            catch (Exception e) {
            }
            var a = @" 
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BitPropertyAttribute : Attribute {
    public readonly Type Type;
    public readonly string PropertyName;
    public readonly int StartIndex;
    public readonly int EndIndex;

    /// <summary>
    /// 
    /// </summary>
    /// <param name=""type"">int or bool</param>
    /// <param name=""propertyName""></param>
    /// <param name=""startIndex"">must be less or equal to endIndex</param>
    /// <param name=""endIndex"">max value is 7</param>
    public BitPropertyAttribute(Type type, string propertyName, int startIndex, int endIndex) {
        Type = type;
        PropertyName = propertyName;
        StartIndex = startIndex;
        EndIndex = endIndex;
        if (StartIndex > EndIndex)
            StartIndex = EndIndex;
    }
}
";
            context.AddSource($"BitPropertyAttribute", SourceText.From(a, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
