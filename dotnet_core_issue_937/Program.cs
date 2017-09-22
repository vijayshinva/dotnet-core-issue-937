using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace dotnet_core_issue_937
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = RazorEngine.Create((configure) =>
            {
                configure.SetBaseType("TemplateBase");
            });
            var project = RazorProject.Create(@".");
            var templateEngine = new RazorTemplateEngine(engine, project);
            templateEngine.Options.DefaultImports = GetDefaultImports();
            var razorCodeDocument = templateEngine.GenerateCode("report.cshtml");
            var metaDataReferences = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.StringBuilder).GetTypeInfo().Assembly.Location),
            };
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(razorCodeDocument.GeneratedCode),
                CSharpSyntaxTree.ParseText(@"
                namespace Razor {
                public abstract class TemplateBase
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        public abstract Task ExecuteAsync();

                        public virtual void Write(object value)
                        {
                            stringBuilder.Append(value);
                            stringBuilder.Append(""\r\n"");
                            //stringBuilder.Append(Environment.NewLine);
                        }

                        public virtual void WriteLiteral(object value)
                        {
                            stringBuilder.Append(value);
                        }
                    }
                }")
            };
            var syntaxTree = CSharpSyntaxTree.ParseText(razorCodeDocument.GeneratedCode);
            var fileName = $"razor_{Guid.NewGuid().ToString("N")}.dll";
            var dirName = @"D:\RepoOfRepos\__JUNK__\";
            var compilation = CSharpCompilation.Create(fileName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(syntaxTrees)
                .AddReferences(metaDataReferences);
            var emitResult = compilation.Emit(Path.Combine(dirName, fileName));

            if (!emitResult.Success)
            {
                Console.WriteLine("------------------------ ERROR ------------------------");
                foreach (var item in emitResult.Diagnostics)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("------------------------ SUCCESS ------------------------");
            }
        }
        private static RazorSourceDocument GetDefaultImports()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine("@using System");
                //writer.WriteLine("@using System.Linq");
                //writer.WriteLine("@using System.Collections.Generic");
                //writer.WriteLine("@using System.Text");
                //writer.WriteLine("@using System.Threading.Tasks");
                writer.Flush();

                stream.Position = 0;
                return RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
            }
        }
    }
}
