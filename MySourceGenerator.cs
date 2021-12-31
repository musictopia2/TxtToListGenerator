namespace TxtToListGenerator;
[Generator]
public class MySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var firsts = context.AdditionalTextsProvider
           .Where(xx => Path.GetExtension(xx.Path) == ".txt");
        var nexts = firsts.Collect();
        var finals = context.CompilationProvider.Combine(nexts);
        context.RegisterSourceOutput(finals, (spc, source) =>
        {
            Execute(source.Left, spc, source.Right);
        });
    }
    private BasicList<FileInformation> GetFiles(ImmutableArray<AdditionalText> files)
    {
        BasicList<FileInformation> output = new();
        foreach (AdditionalText file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file.Path);
            string details = file.GetText()!.ToString();
            BasicList<string> list = details.Split(Environment.NewLine);
            output.Add(new(name, list));
        }
        return output;
    }
    private void Execute(Compilation compilation, SourceProductionContext context, ImmutableArray<AdditionalText> list)
    {
        var files = GetFiles(list);
        var name = $"{compilation.AssemblyName}.Resources";
        files.ForEach(file =>
        {
            string className = file.FileName;
            string fins = $"{className}.g.cs";
            SourceCodeStringBuilder builder = new();
            builder.WriteLine(w =>
            {
                w.Write("namespace ")
                .Write(name)
                .Write(";");
            })
            .WriteLine(w =>
            {
                w.Write("public static class ")
                .Write(className);
            })
            .WriteCodeBlock(w =>
            {
                var temps = file.List.CastStringListToIntegerList();
                w.WriteLine(w =>
                {
                    w.Write("public static global::CommonBasicLibraries.CollectionClasses.BasicList");
                    if (temps.Count == 0)
                    {
                        w.SingleGenericWrite("string");
                    }
                    else
                    {
                        w.SingleGenericWrite("int");
                    }
                    w.Write(" GetTextList()");
                })
                .WriteCodeBlock(w =>
                {

                    if (temps.Count > 0)
                    {
                        w.InitializeNewBasicList(temps, "output");
                    }
                    else
                    {
                        w.InitializeNewBasicList(file.List, "output");
                    }
                    w.WriteLine("return output;");
                });
            });
            context.AddSource(fins, builder.ToString());
        });
    }
}